﻿using EspapMiddleware.ServiceLayer.Helpers.OutboundMessageInspector;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Exceptions;
using EspapMiddleware.Shared.Interfaces.IConfiguration;
using EspapMiddleware.Shared.Interfaces.IHelpers;
using EspapMiddleware.Shared.Interfaces.IServices;
using EspapMiddleware.Shared.MonitorServiceModels;
using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using EspapMiddleware.Shared.WebServiceModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace EspapMiddleware.ServiceLayer.Services
{
    public class MonitorService : IMonitorService
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGenericRestRequestManager _genericRestRequestManager;

        public MonitorService(IUnitOfWorkFactory unitOfWorkFactory, IGenericRestRequestManager genericRestRequestManager)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRestRequestManager = genericRestRequestManager;
        }

        #region RequestLogs
        public async Task<PaginatedResult<RequestLog>> RequestLogSearch(RequestLogSearchFilters filters)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return new PaginatedResult<RequestLog>()
                {
                    PageIndex = filters.PageIndex,
                    PageSize = filters.PageSize,
                    TotalCount = await unitOfWork.RequestLogs.Count(x => (!filters.UniqueId.HasValue || x.UniqueId == filters.UniqueId)
                                                                    && (!filters.Type.HasValue || x.RequestLogTypeId == filters.Type)
                                                                    && (string.IsNullOrEmpty(filters.ReferenceNumber) || x.ReferenceNumber.Contains(filters.ReferenceNumber))
                                                                    && (string.IsNullOrEmpty(filters.SupplierFiscalId) || x.SupplierFiscalId.Contains(filters.SupplierFiscalId))
                                                                    && (!filters.IsSuccessFul.HasValue || x.Successful == filters.IsSuccessFul)
                                                                    && (!filters.FromDate.HasValue || x.Date >= filters.FromDate)
                                                                    && (!filters.UntilDate.HasValue || x.Date <= filters.UntilDate)),
                    Data = await unitOfWork.RequestLogs.GetFilteredPaginated(filters)
                };
        }

        public async Task<RequestLog> GetLogForDownload(Guid uniqueId, RequestLogTypeEnum type)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.RequestLogs.Find(x => x.UniqueId == uniqueId && x.RequestLogTypeId == type);
        }

        #endregion


        #region Documents
        public async Task<PaginatedResult<Document>> DocumentSearch(DocumentSearchFilters filters)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return new PaginatedResult<Document>()
                {
                    PageIndex = filters.PageIndex,
                    PageSize = filters.PageSize,
                    TotalCount = await unitOfWork.Documents.Count(x => (string.IsNullOrEmpty(filters.DocumentId) || x.DocumentId == filters.DocumentId)
                                        && (!filters.FromDate.HasValue || x.CreatedOn >= filters.FromDate)
                                        && (!filters.UntilDate.HasValue || x.CreatedOn <= filters.UntilDate)
                                        && (string.IsNullOrEmpty(filters.SupplierFiscalId) || x.SupplierFiscalId.Contains(filters.SupplierFiscalId))
                                        && (string.IsNullOrEmpty(filters.SchoolYear) || x.SchoolYear == filters.SchoolYear)
                                        && (string.IsNullOrEmpty(filters.CompromiseNumber) || x.CompromiseNumber.Contains(filters.CompromiseNumber))
                                        && (string.IsNullOrEmpty(filters.ReferenceNumber) || x.ReferenceNumber.Contains(filters.ReferenceNumber))
                                        && (!filters.State.HasValue || x.StateId == filters.State)
                                        && (!filters.Type.HasValue || x.TypeId == filters.Type)
                                        && (string.IsNullOrEmpty(filters.MeId) || x.MEId == filters.MeId)
                                        && (!filters.SigefeSyncronized.HasValue || x.IsSynchronizedWithSigefe == filters.SigefeSyncronized)
                                        && (!filters.FeapSyncronized.HasValue || x.IsSynchronizedWithFEAP == filters.FeapSyncronized)),
                    Data = await unitOfWork.Documents.GetFilteredPaginated(filters)
                };
        }

        public async Task<Document> GetDocumentDetail(string documentId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.Documents.GetDocumentForDetail(documentId);
        }

        public async Task<PaginatedResult<DocumentLine>> GetDocumentLinesForDetail(DocumentDetailLineFilter filters)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return new PaginatedResult<DocumentLine>()
                {
                    PageIndex = filters.PageIndex,
                    PageSize = filters.PageSize,
                    TotalCount = await unitOfWork.DocumentLines.Count(x => x.DocumentId == filters.DocumentId),
                    Data = await unitOfWork.DocumentLines.GetFilteredPaginated(filters)
                };
        }

        public async Task<IEnumerable<string>> GetSchoolYears()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.Documents.GetSchoolYears();
        }

        public async Task SyncSigefe(string documentId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docToSync = await unitOfWork.Documents.GetDocumentForSyncSigefe(documentId);

                if (docToSync.IsSynchronizedWithSigefe)
                    throw new Exception("Documento já se encontra sincronizado.");

                if (string.IsNullOrEmpty(docToSync.SchoolYear))
                {
                    var getFaseResponse = await _genericRestRequestManager.Get<GetFaseResponse>("getFase");
                    docToSync.SchoolYear = getFaseResponse?.id_ano_letivo_atual;
                }

                var setDocFaturacaoResponse = await RequestSetDocFaturacao(docToSync);

                //FALHA DE COMUNICAÇÃO
                if (setDocFaturacaoResponse == null)
                    throw new Exception("Erro de comunicação com SIGeFE. Tentar mais tarde");

                //CHAMADA WEBSERVICE BEM SUCEDIDA
                if (setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "200"))
                {
                    var docToSyncResult = setDocFaturacaoResponse.faturas.FirstOrDefault();

                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                    {
                        DocumentId = docToSync.DocumentId,
                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                        Date = DateTime.Now,
                        MessageCode = docToSyncResult.cod_msg_fat,
                        MessageContent = docToSyncResult.msg_fat
                    });

                    docToSync.MEId = docToSyncResult.id_me_fatura;

                    //ATUALIZAR COMPROMISSO COM O VALOR TRATADO
                    if (!string.IsNullOrEmpty(docToSyncResult.num_compromisso))
                        docToSync.CompromiseNumber = docToSyncResult.num_compromisso;

                    docToSync.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(docToSyncResult.id_me_fatura);
                    docToSync.IsSynchronizedWithFEAP = !docToSync.IsSynchronizedWithSigefe;

                    switch (docToSync.TypeId)
                    {
                        case DocumentTypeEnum.Fatura:
                            if (docToSyncResult.state_id == "35")
                            {
                                docToSync.StateId = DocumentStateEnum.ValidadoConferido;
                                docToSync.StateDate = DateTime.Now;

                            }
                            else if (docToSyncResult.state_id == "22")
                            {
                                docToSync.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                docToSync.ActionDate = DateTime.Now;
                            }

                            break;
                        case DocumentTypeEnum.NotaCrédito:
                            if (docToSyncResult.cod_msg_fat != "490")
                            {
                                docToSync.RelatedReferenceNumber = docToSyncResult.num_doc_rel;
                                docToSync.RelatedDocumentId = docToSyncResult.id_doc_feap_rel;

                                if (docToSyncResult.state_id == "35")
                                {
                                    docToSync.StateId = DocumentStateEnum.Processado;
                                    docToSync.StateDate = DateTime.Now;

                                    var relatedDocumentToupdate = await unitOfWork.Documents.Find(x => x.DocumentId == docToSync.RelatedDocumentId);

                                    if (relatedDocumentToupdate != null)
                                    {
                                        relatedDocumentToupdate.StateId = DocumentStateEnum.Processado;
                                        relatedDocumentToupdate.StateDate = DateTime.Now;
                                        relatedDocumentToupdate.IsSynchronizedWithFEAP = false;

                                        unitOfWork.Documents.Update(relatedDocumentToupdate);
                                    }
                                }
                                else if (docToSyncResult.state_id == "22")
                                {
                                    docToSync.StateId = DocumentStateEnum.Devolvido;
                                    docToSync.ActionDate = DateTime.Now;
                                }
                            }

                            break;
                        case DocumentTypeEnum.NotaDébito:
                            docToSync.StateId = DocumentStateEnum.Devolvido;
                            docToSync.ActionDate = DateTime.Now;

                            docToSync.IsSynchronizedWithSigefe = true;
                            docToSync.IsSynchronizedWithFEAP = false;
                            
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                    {
                        DocumentId = docToSync.DocumentId,
                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                        Date = DateTime.Now,
                        MessageCode = setDocFaturacaoResponse.messages.FirstOrDefault()?.cod_msg,
                        MessageContent = setDocFaturacaoResponse.messages.FirstOrDefault()?.msg
                    });
                }

                unitOfWork.Documents.Update(docToSync);

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro ao atualizar documento na BD.");
                }
            }
        }

        public async Task SyncFeap(string documentId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docToSync = await unitOfWork.Documents.Find(x => x.DocumentId == documentId);

                if (docToSync.IsSynchronizedWithFEAP)
                    throw new Exception("Documento já se encontra sincronizado.");
                if (!docToSync.IsSynchronizedWithFEAP && !docToSync.IsSynchronizedWithSigefe)
                    throw new Exception("Documento necessita ser sicronizado com SIGeFE primeiro.");

                var lastSigefeMessage = await unitOfWork.DocumentMessages.GetLastSigefeMessage(documentId);

                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, lastSigefeMessage?.MessageContent));

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro de comunicação com BD.");
                }
            }
        }

        public async Task ReturnDocument(string documentId, string reason)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docToReturn = await unitOfWork.Documents.Find(x => x.DocumentId == documentId);

                if (docToReturn.StateId == DocumentStateEnum.Devolvido 
                    || docToReturn.StateId == DocumentStateEnum.EmitidoPagamento)
                    throw new Exception("Não é possível devolver o documento.");

                docToReturn.StateId = DocumentStateEnum.Devolvido;
                docToReturn.StateDate = DateTime.Now;
                docToReturn.IsSynchronizedWithFEAP = false;

                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToReturn, reason));

                unitOfWork.Documents.Update(docToReturn);

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro de comunicação com BD.");
                }
            }
        }

        public async Task ResetCompromiseNumber(string documentId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docToReset = await unitOfWork.Documents.Find(x => x.DocumentId == documentId);

                var setEstadoDocFaturacaoObj = new SetEstadoDocFaturacao()
                {
                    estado_doc = "7",
                    id_ano_letivo = docToReset.SchoolYear,
                    id_me_fatura = docToReset.MEId,
                    nif = docToReset.SupplierFiscalId.Substring(2)
                };

                var setEstadoDocFaturacaoResponse = await _genericRestRequestManager.Post<GenericPostResponse, SetEstadoDocFaturacao>("setEstadoDocFaturacao", setEstadoDocFaturacaoObj);

                if (setEstadoDocFaturacaoResponse == null)
                    throw new Exception("Falha de comunicação. Reenviar pedido mais tarde.");

                unitOfWork.DocumentMessages.Add(new DocumentMessage()
                {
                    DocumentId = docToReset.DocumentId,
                    MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                    Date = DateTime.Now,
                    MessageCode = setEstadoDocFaturacaoResponse.messages.FirstOrDefault()?.cod_msg,
                    MessageContent = setEstadoDocFaturacaoResponse.messages.FirstOrDefault()?.msg
                });

                unitOfWork.Documents.Update(docToReset);

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro de comunicação com BD.");
                }
            }
        }

        public async Task ResetSigefeSync(string documentId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docToReset = await unitOfWork.Documents.Find(x => x.DocumentId == documentId);

                docToReset.IsSynchronizedWithSigefe = false;
                docToReset.MEId = null;
                docToReset.StateId = DocumentStateEnum.Iniciado;
                docToReset.ActionId = null;

                unitOfWork.Documents.Update(docToReset);

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro de comunicação com BD.");
                }
            }
        }

        #endregion


        #region Homepage

        public async Task<string> GetCurrentSchoolYear()
        {
            var getFaseResponse = await _genericRestRequestManager.Get<GetFaseResponse>("getFase");

            return getFaseResponse?.id_ano_letivo_atual;
        }

        public async Task<int> GetTotalDocument(string anoLetivo, bool? isSynchronizedWithFEAP = null)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.Documents.Count(x => x.SchoolYear == anoLetivo && (!isSynchronizedWithFEAP.HasValue || x.IsSynchronizedWithFEAP == isSynchronizedWithFEAP.Value));
        }

        public async Task<int> GetTotalDocumentsByType(string anoLetivo, DocumentTypeEnum typeId, DocumentStateEnum? stateId = null, DocumentActionEnum? actionId = null)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.Documents.Count(x => x.SchoolYear == anoLetivo
                                                        && x.TypeId == typeId
                                                        && (!actionId.HasValue || x.ActionId == actionId.Value)
                                                        && (!stateId.HasValue || x.StateId == stateId.Value));
        }

        public async Task SyncAllDocumentsFeap(string anoLetivo)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var errors = new List<string>();

                var docsToSync = await unitOfWork.Documents.GetDocumentsToSyncFeap(anoLetivo);

                foreach (var doc in docsToSync)
                {
                    try
                    {
                        var lastSigefeMessage = doc.DocumentMessages
                                                    .Where(x => x.MessageTypeId == DocumentMessageTypeEnum.SIGeFE)
                                                    .OrderByDescending(x => x.Date)
                                                    .FirstOrDefault()?.MessageContent;

                        var setDocumentRequestLog = await RequestSetDocument(doc, lastSigefeMessage);

                        unitOfWork.RequestLogs.Add(setDocumentRequestLog);

                        await unitOfWork.SaveChangesAsync();

                        if (!setDocumentRequestLog.Successful)
                            throw new Exception("Falha de comunicação com FEAP.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{doc.DocumentId} - {ex.GetBaseException().Message}");
                    }
                }

                if (errors.Count != 0)
                    throw new SyncronizationException(errors.ToArray());
            }
        }

        public async Task<PaginatedResult<Document>> GetDocsToSyncSigefe(PaginatedSearchFilter filters)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var getFaseResponse = await _genericRestRequestManager.Get<GetFaseResponse>("getFase");

                if (getFaseResponse == null)
                    throw new Exception("Ano letivo não disponível.");

                return new PaginatedResult<Document>()
                {
                    PageIndex = filters.PageIndex,
                    PageSize = filters.PageSize,
                    TotalCount = await unitOfWork.Documents.GetDocsToSyncSigefeCount(getFaseResponse.id_ano_letivo_atual),
                    Data = await unitOfWork.Documents.GetPaginatedDocsToSyncSigefe(getFaseResponse.id_ano_letivo_atual, filters),
                };
            }
        }

        public async Task SyncDocumentsSigefe(string documentId = null)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var getFaseResponse = await _genericRestRequestManager.Get<GetFaseResponse>("getFase");

                if (getFaseResponse == null)
                    throw new Exception("Ano letivo não disponível.");

                var docsToSyncSigefe = await unitOfWork.Documents.GetDocsToSyncSigefe(getFaseResponse.id_ano_letivo_atual, documentId);

                var errors = new List<string>();

                foreach (var docToSync in docsToSyncSigefe)
                {
                    try
                    {
                        docToSync.SchoolYear = getFaseResponse.id_ano_letivo_atual;

                        var setDocFaturacaoResponse = await RequestSetDocFaturacao(docToSync);

                        //FALHA DE COMUNICAÇÃO
                        if (setDocFaturacaoResponse == null)
                        {
                            unitOfWork.DocumentMessages.Add(new DocumentMessage()
                            {
                                DocumentId = docToSync.DocumentId,
                                MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                Date = DateTime.Now,
                                MessageCode = "500",
                                MessageContent = "Falha de comunicação. Reenviar pedido mais tarde."
                            });
                        }
                        else
                        {
                            //CHAMADA WEBSERVICE BEM SUCEDIDA
                            if (setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "200"))
                            {
                                var docToSyncResult = setDocFaturacaoResponse.faturas.FirstOrDefault();

                                unitOfWork.DocumentMessages.Add(new DocumentMessage()
                                {
                                    DocumentId = docToSync.DocumentId,
                                    MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                    Date = DateTime.Now,
                                    MessageCode = docToSyncResult.cod_msg_fat,
                                    MessageContent = docToSyncResult.msg_fat
                                });

                                docToSync.MEId = docToSyncResult.id_me_fatura;

                                if (!string.IsNullOrEmpty(docToSyncResult.num_compromisso))
                                    docToSync.CompromiseNumber = docToSyncResult.num_compromisso;

                                docToSync.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(docToSyncResult.id_me_fatura);
                                docToSync.IsSynchronizedWithFEAP = !docToSync.IsSynchronizedWithSigefe;

                                switch (docToSync.TypeId)
                                {
                                    case DocumentTypeEnum.Fatura:
                                        if (docToSyncResult.state_id == "35")
                                        {
                                            docToSync.StateId = DocumentStateEnum.ValidadoConferido;
                                            docToSync.StateDate = DateTime.Now;

                                            unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync));
                                        }
                                        else if (docToSyncResult.state_id == "22")
                                        {
                                            docToSync.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                            docToSync.ActionDate = DateTime.Now;

                                            unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSyncResult.reason));
                                        }

                                        break;
                                    case DocumentTypeEnum.NotaCrédito:
                                        if (docToSyncResult.cod_msg_fat != "490")
                                        {
                                            docToSync.RelatedReferenceNumber = docToSyncResult.num_doc_rel;
                                            docToSync.RelatedDocumentId = docToSyncResult.id_doc_feap_rel;

                                            if (docToSyncResult.state_id == "35")
                                            {
                                                docToSync.StateId = DocumentStateEnum.Processado;
                                                docToSync.StateDate = DateTime.Now;

                                                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync));

                                                var relatedDocumentToupdate = await unitOfWork.Documents.Find(x => x.DocumentId == docToSync.RelatedDocumentId);

                                                if (relatedDocumentToupdate != null)
                                                {
                                                    relatedDocumentToupdate.StateId = DocumentStateEnum.Processado;
                                                    relatedDocumentToupdate.StateDate = DateTime.Now;
                                                    relatedDocumentToupdate.IsSynchronizedWithFEAP = false;

                                                    unitOfWork.Documents.Update(relatedDocumentToupdate);

                                                    unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocumentToupdate));
                                                }
                                            }
                                            else if (docToSyncResult.state_id == "22")
                                            {
                                                docToSync.StateId = DocumentStateEnum.Devolvido;
                                                docToSync.ActionDate = DateTime.Now;

                                                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSyncResult.reason));
                                            }
                                        }

                                        break;
                                    case DocumentTypeEnum.NotaDébito:
                                        docToSync.IsSynchronizedWithSigefe = true;
                                        docToSync.IsSynchronizedWithFEAP = false;

                                        docToSync.StateId = DocumentStateEnum.Devolvido;
                                        docToSync.ActionDate = DateTime.Now;

                                        unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSyncResult.reason));

                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                unitOfWork.DocumentMessages.Add(new DocumentMessage()
                                {
                                    DocumentId = docToSync.DocumentId,
                                    MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                    Date = DateTime.Now,
                                    MessageCode = setDocFaturacaoResponse.messages.FirstOrDefault()?.cod_msg,
                                    MessageContent = setDocFaturacaoResponse.messages.FirstOrDefault()?.msg
                                });
                            }
                        }

                        unitOfWork.Documents.Update(docToSync);

                        var result = await unitOfWork.SaveChangesAsync();

                        if (result == 0)
                            throw new DatabaseException("Erro ao atualizar documento na BD.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{docToSync.DocumentId} - {ex.GetBaseException().Message}");
                    }
                }

                if (errors.Count != 0)
                    throw new SyncronizationException(errors.ToArray());
            }
        }

        public async Task<int> GetTotalPaidDocsToSync()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var GetDocFaturacaoResponse = await GetDocFaturacao("4");

                if (GetDocFaturacaoResponse == null)
                    throw new WebserviceException("Erro na chamada ao serviço de Faturação do SIGeFE.");

                var documentIds = GetDocFaturacaoResponse.documentos.Select(x => x.id_doc_feap).ToArray();

                return await unitOfWork.Documents.GetPaidDocsToSyncCount(documentIds);
            }
        }

        public async Task SyncPaidDocuments()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var GetDocFaturacaoResponse = await GetDocFaturacao("4");

                if (GetDocFaturacaoResponse == null)
                    throw new WebserviceException("Erro na chamada ao serviço de Faturação do SIGeFE.");

                var errors = new List<string>();

                var documentIds = GetDocFaturacaoResponse.documentos.Select(x => x.id_doc_feap).ToArray();

                var paidDocsToSync = await unitOfWork.Documents.GetPaidDocsToSync(documentIds);

                foreach (var document in paidDocsToSync)
                {
                    try
                    {
                        document.StateId = DocumentStateEnum.EmitidoPagamento;
                        document.StateDate = DateTime.Now;
                        document.IsSynchronizedWithFEAP = false;

                        unitOfWork.Documents.Update(document);

                        var setDocumentRequestLog = await RequestSetDocument(document);

                        unitOfWork.RequestLogs.Add(setDocumentRequestLog);

                        await unitOfWork.SaveChangesAsync();

                        if (!setDocumentRequestLog.Successful)
                        {
                            throw new Exception("Falha de comunicação com FEAP.");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{document.DocumentId} - {ex.GetBaseException().Message}");
                    }
                }

                if (errors.Count != 0)
                    throw new SyncronizationException(errors.ToArray());
            }
        }

        public async Task<int> GetTotalCreditNotesToReprocess()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var getFaseResponse = await _genericRestRequestManager.Get<GetFaseResponse>("getFase");

                return await unitOfWork.Documents.GetCreditNotesToReprocessCount(getFaseResponse.id_ano_letivo_atual);
            }
        }

        public async Task ReprocessCreditNotes()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var getFaseResponse = await _genericRestRequestManager.Get<GetFaseResponse>("getFase");

                if (getFaseResponse == null)
                    throw new Exception("Ano letivo não disponível.");

                var docsToReprocess = await unitOfWork.Documents.GetCreditNotesToReprocess(getFaseResponse.id_ano_letivo_atual);

                var errors = new List<string>();

                foreach (var docToReprocess in docsToReprocess)
                {
                    try
                    {
                        var setDocFaturacaoResponse = await RequestSetDocFaturacao(docToReprocess);

                        //FALHA DE COMUNICAÇÃO
                        if (setDocFaturacaoResponse == null)
                        {
                            unitOfWork.DocumentMessages.Add(new DocumentMessage()
                            {
                                DocumentId = docToReprocess.DocumentId,
                                MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                Date = DateTime.Now,
                                MessageCode = "500",
                                MessageContent = "Falha de comunicação. Reenviar pedido mais tarde."
                            });
                        }
                        else
                        {
                            //CHAMADA WEBSERVICE BEM SUCEDIDA
                            if (setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "200"))
                            {
                                var docToReprocessResult = setDocFaturacaoResponse.faturas.FirstOrDefault();

                                unitOfWork.DocumentMessages.Add(new DocumentMessage()
                                {
                                    DocumentId = docToReprocess.DocumentId,
                                    MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                    Date = DateTime.Now,
                                    MessageCode = docToReprocessResult.cod_msg_fat,
                                    MessageContent = docToReprocessResult.msg_fat
                                });

                                docToReprocess.MEId = docToReprocessResult.id_me_fatura;

                                if (!string.IsNullOrEmpty(docToReprocessResult.num_compromisso))
                                    docToReprocess.CompromiseNumber = docToReprocessResult.num_compromisso;

                                docToReprocess.IsSynchronizedWithSigefe = true;
                                docToReprocess.IsSynchronizedWithFEAP = false;


                                if (docToReprocessResult.cod_msg_fat == "490")
                                {
                                    docToReprocess.StateId = DocumentStateEnum.Devolvido;
                                    docToReprocess.ActionDate = DateTime.Now;

                                    unitOfWork.RequestLogs.Add(await RequestSetDocument(docToReprocess, docToReprocessResult.msg_fat));
                                }
                                else
                                {
                                    docToReprocess.RelatedReferenceNumber = docToReprocessResult.num_doc_rel;
                                    docToReprocess.RelatedDocumentId = docToReprocessResult.id_doc_feap_rel;

                                    if (docToReprocessResult.state_id == "35")
                                    {
                                        docToReprocess.StateId = DocumentStateEnum.Processado;
                                        docToReprocess.StateDate = DateTime.Now;

                                        unitOfWork.RequestLogs.Add(await RequestSetDocument(docToReprocess));

                                        var relatedDocumentToupdate = await unitOfWork.Documents.Find(x => x.DocumentId == docToReprocess.RelatedDocumentId);

                                        if (relatedDocumentToupdate != null)
                                        {
                                            relatedDocumentToupdate.StateId = DocumentStateEnum.Processado;
                                            relatedDocumentToupdate.StateDate = DateTime.Now;
                                            relatedDocumentToupdate.IsSynchronizedWithFEAP = false;

                                            unitOfWork.Documents.Update(relatedDocumentToupdate);

                                            unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocumentToupdate));
                                        }
                                    }
                                    else if (docToReprocessResult.state_id == "22")
                                    {
                                        docToReprocess.StateId = DocumentStateEnum.Devolvido;
                                        docToReprocess.ActionDate = DateTime.Now;

                                        unitOfWork.RequestLogs.Add(await RequestSetDocument(docToReprocess, docToReprocessResult.reason));
                                    }
                                }
                            }
                            else
                            {
                                unitOfWork.DocumentMessages.Add(new DocumentMessage()
                                {
                                    DocumentId = docToReprocess.DocumentId,
                                    MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                    Date = DateTime.Now,
                                    MessageCode = setDocFaturacaoResponse.messages.FirstOrDefault()?.cod_msg,
                                    MessageContent = setDocFaturacaoResponse.messages.FirstOrDefault()?.msg
                                });
                            }
                        }

                        unitOfWork.Documents.Update(docToReprocess);

                        var result = await unitOfWork.SaveChangesAsync();

                        if (result == 0)
                            throw new DatabaseException("Erro ao atualizar documento na BD.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{docToReprocess.DocumentId} - {ex.GetBaseException().Message}");
                    }
                }

                if (errors.Count != 0)
                    throw new SyncronizationException(errors.ToArray());
            }
        }

        public async Task ReturnDebitNotes()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var getFaseResponse = await _genericRestRequestManager.Get<GetFaseResponse>("getFase");

                if (getFaseResponse == null)
                    throw new Exception("Ano letivo não disponível.");

                var debitNotesToReturn = await unitOfWork.Documents.GetFiltered(x => x.SchoolYear == getFaseResponse.id_ano_letivo_atual
                                                                            && x.StateId == DocumentStateEnum.Iniciado
                                                                            && x.TypeId == DocumentTypeEnum.NotaDébito);
                
                var errors = new List<string>();

                foreach (var debitNoteToReturn in debitNotesToReturn)
                {
                    try
                    {
                        debitNoteToReturn.StateId = DocumentStateEnum.Devolvido;
                        debitNoteToReturn.StateDate = DateTime.Now;

                        debitNoteToReturn.IsSynchronizedWithSigefe = true;
                        debitNoteToReturn.IsSynchronizedWithFEAP = false;

                        var setDocumentRequestLog = await RequestSetDocument(debitNoteToReturn, "Documentos do tipo Nota de Débito não são compativeis com o programa MEGA.");
                        
                        unitOfWork.RequestLogs.Add(setDocumentRequestLog);

                        unitOfWork.Documents.Update(debitNoteToReturn);

                        var result = await unitOfWork.SaveChangesAsync();

                        if (!setDocumentRequestLog.Successful)
                            throw new Exception("Falha de comunicação com FEAP.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{debitNoteToReturn.DocumentId} - {ex.GetBaseException().Message}");
                    }
                }

                if (errors.Count != 0)
                    throw new SyncronizationException(errors.ToArray());
            }
        }

        #endregion


        #region Private Methods

        private async Task<GetDocFaturacaoResponse> GetDocFaturacao(string estado_doc, string nif = null, string id_doc_feap = null)
        {
            var headers = new Dictionary<string, string>();

            var getFaseResponse = await _genericRestRequestManager.Get<GetFaseResponse>("getFase");

            headers.Add("id_ano_letivo", getFaseResponse?.id_ano_letivo_atual);

            headers.Add("estado_doc", estado_doc);

            if (!string.IsNullOrEmpty(nif))
                headers.Add("nif", nif);

            if (!string.IsNullOrEmpty(id_doc_feap))
                headers.Add("id_doc_feap", id_doc_feap);

            return await _genericRestRequestManager.Get<GetDocFaturacaoResponse>("getDocFaturacao", headers);
        }

        private async Task<RequestLog> RequestSetDocument(Document document, string reason = null)
        {
            var uniqueId = Guid.NewGuid();

            try
            {
                using (var client = new FEAPServices_PP.FEAPServicesClient())
                {
                    client.Endpoint.Behaviors.Add(new EndpointBehavior(uniqueId.ToString()));

                    var feapCredencials = (NameValueCollection)ConfigurationManager.GetSection("FEAPCredencials");

                    client.ClientCredentials.UserName.UserName = feapCredencials["username"];
                    client.ClientCredentials.UserName.Password = feapCredencials["password"];

                    var serviceContextHeader = new FEAPServices_PP.ServiceContextHeader()
                    {
                        Application = "FaturacaoEletronica",
                        ProcessId = document.DocumentId
                    };

                    var setDocumentRequest = new FEAPServices_PP.SetDocumentRequest()
                    {
                        uniqueId = uniqueId.ToString(),
                        documentId = document.DocumentId,
                    };

                    switch (document.StateId)
                    {
                        case DocumentStateEnum.Iniciado:
                            if (document.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização)
                            {
                                setDocumentRequest.actionIdSpecified = true;
                                setDocumentRequest.actionId = (int)document.ActionId;
                                setDocumentRequest.regularizationSpecified = true;

                                switch (document.TypeId)
                                {
                                    case DocumentTypeEnum.Fatura:
                                        setDocumentRequest.regularization = 2;
                                        break;
                                    case DocumentTypeEnum.NotaCrédito:
                                        setDocumentRequest.regularization = 3;
                                        break;
                                    case DocumentTypeEnum.NotaDébito:
                                        setDocumentRequest.regularization = 2;
                                        break;
                                    default:
                                        break;
                                }

                                setDocumentRequest.reason = reason;
                            }
                            break;
                        case DocumentStateEnum.ValidadoConferido:
                            setDocumentRequest.stateIdSpecified = true;
                            setDocumentRequest.stateId = (int)document.StateId;

                            setDocumentRequest.documentNumbersSpecified1Specified = true;
                            setDocumentRequest.documentNumbersSpecified1 = true;
                            setDocumentRequest.documentNumbers = new string[] { document.MEId };
                            break;
                        case DocumentStateEnum.Processado:
                            setDocumentRequest.stateIdSpecified = true;
                            setDocumentRequest.stateId = (int)document.StateId;

                            setDocumentRequest.documentNumbersSpecified1Specified = true;
                            setDocumentRequest.documentNumbersSpecified1 = true;
                            setDocumentRequest.documentNumbers = new string[] { document.MEId };

                            setDocumentRequest.commitmentSpecified1Specified = true;
                            setDocumentRequest.commitmentSpecified1 = true;
                            setDocumentRequest.commitment = !string.IsNullOrEmpty(document.CompromiseNumber) ? document.CompromiseNumber : "N/A";

                            setDocumentRequest.postingDateSpecified1Specified = true;
                            setDocumentRequest.postingDateSpecified1 = true;
                            setDocumentRequest.postingDateSpecified = true;
                            setDocumentRequest.postingDate = DateTime.Now;
                            break;
                        case DocumentStateEnum.EmitidoPagamento:
                            setDocumentRequest.stateIdSpecified = true;
                            setDocumentRequest.stateId = (int)document.StateId;

                            setDocumentRequest.paymentIssuedReference = document.MEId;
                            setDocumentRequest.paymentIssuedReferenceSpecified1 = true;
                            setDocumentRequest.paymentIssuedReferenceSpecified1Specified = true;

                            setDocumentRequest.paymentIssuedDate = document.IssueDate;
                            setDocumentRequest.paymentIssuedDateSpecified = true;
                            setDocumentRequest.paymentIssuedDateSpecified1 = true;
                            setDocumentRequest.paymentIssuedDateSpecified1Specified = true;

                            setDocumentRequest.paymentReference = document.ReferenceNumber;
                            setDocumentRequest.paymentReferenceSpecified1 = true;
                            setDocumentRequest.paymentReferenceSpecified1Specified = true;
                            break;
                        case DocumentStateEnum.Devolvido:
                            setDocumentRequest.stateIdSpecified = true;
                            setDocumentRequest.stateId = (int)document.StateId;

                            setDocumentRequest.reason = reason;
                            break;
                        default:
                            break;
                    }

                    await client.SetDocumentAsync(serviceContextHeader, setDocumentRequest);

                    return new RequestLog()
                    {
                        UniqueId = uniqueId,
                        RequestLogTypeId = RequestLogTypeEnum.SetDocument,
                        DocumentId = document.DocumentId,
                        SupplierFiscalId = document.SupplierFiscalId,
                        ReferenceNumber = document.ReferenceNumber,
                        Date = DateTime.Now,
                        Successful = true
                    };
                }
            }
            catch (Exception ex)
            {
                return new RequestLog()
                {
                    UniqueId = uniqueId,
                    RequestLogTypeId = RequestLogTypeEnum.SetDocument,
                    DocumentId = document.DocumentId,
                    SupplierFiscalId = document.SupplierFiscalId,
                    ReferenceNumber = document.ReferenceNumber,
                    Date = DateTime.Now,
                    Successful = false,
                    ExceptionType = ex.GetBaseException().GetType().Name,
                    ExceptionStackTrace = ex.GetBaseException().StackTrace,
                    ExceptionMessage = ex.GetBaseException().Message
                };
            }
        }

        private async Task<SetDocFaturacaoResponse> RequestSetDocFaturacao(Document document)
        {
            var setDocFaturacaoObj = new SetDocFaturacao();

            FillWithContract(in document, ref setDocFaturacaoObj);

            var setDocFaturacaoResult = await _genericRestRequestManager.Post<SetDocFaturacaoResponse, SetDocFaturacao>("setDocFaturacao", setDocFaturacaoObj);

            return setDocFaturacaoResult;
        }

        private void FillWithContract(in Document document, ref SetDocFaturacao obj)
        {
            obj.id_ano_letivo = document.SchoolYear;

            obj.nif = document.SupplierFiscalId.Substring(2);

            var faturaToSend = new SetDocFaturacao.fatura()
            {
                id_doc_feap = document.DocumentId,
                id_me_fatura = document.MEId,
                num_fatura = document.ReferenceNumber,
                total_fatura = document.TotalAmount,
                fatura_base64 = Convert.ToBase64String(document.PdfFormat),
                dt_fatura = document.IssueDate.ToString("dd-MM-yyyy"),
                num_compromisso = document.CompromiseNumber,
            };

            switch (document.TypeId)
            {
                case DocumentTypeEnum.Fatura:
                    faturaToSend.tp_doc = "FAT";
                    break;
                case DocumentTypeEnum.NotaCrédito:
                    faturaToSend.tp_doc = "NTC";
                    faturaToSend.num_doc_rel = document.RelatedReferenceNumber;
                    break;
                case DocumentTypeEnum.NotaDébito:
                    faturaToSend.tp_doc = "NTD";
                    faturaToSend.num_doc_rel = document.RelatedReferenceNumber;
                    break;
                default:
                    break;
            }

            if (document.StateId == DocumentStateEnum.EmitidoPagamento)
            {
                faturaToSend.estado_doc = "5";
                faturaToSend.dt_estado = document.StateDate.ToString("dd-MM-yyyy");
            }

            faturaToSend.linhas = new List<SetDocFaturacao.fatura.linhaModel>();

            foreach (var line in document.DocumentLines)
            {
                faturaToSend.linhas.Add(new SetDocFaturacao.fatura.linhaModel()
                {
                    id_linha = line.StandardItemIdentification,
                    num_linha = line.LineId,
                    descricao_linha = line.Description,
                    qtd_linha = line.Quantity,
                    valor_linha = line.Value,
                    perc_iva_linha = line.TaxPercentage,
                });
            }

            obj.faturas = new List<SetDocFaturacao.fatura>
            {
                faturaToSend
            };
        }
        #endregion


    }
}
