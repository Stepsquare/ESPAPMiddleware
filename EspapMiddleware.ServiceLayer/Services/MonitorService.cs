using EspapMiddleware.ServiceLayer.Helpers;
using EspapMiddleware.ServiceLayer.Helpers.OutboundMessageInspector;
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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EspapMiddleware.ServiceLayer.Services
{
    public class MonitorService : IMonitorServices
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGenericRestRequestManager _genericRestRequestManager;
        private readonly string _anoLetivoAtual;

        public MonitorService(IUnitOfWorkFactory unitOfWorkFactory, IGenericRestRequestManager genericRestRequestManager)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRestRequestManager = genericRestRequestManager;
            _anoLetivoAtual = ConfigurationManager.AppSettings["AnoLetivoAtual"];
        }

        public async Task<GetFaseResponse> GetFase()
        {
            return await _genericRestRequestManager.Get<GetFaseResponse>("getFase"); ;
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

        public async Task<RequestLogFile> GetRequestLogFile(int id)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.RequestLogFiles.Find(x => x.Id == id);
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
                                        && (!filters.IsMEGA.HasValue || x.IsMEGA == filters.IsMEGA)
                                        && (!filters.IsProcessed.HasValue || x.IsProcessed == filters.IsProcessed)
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

        public async Task SyncSigefe(string documentId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docToSync = await unitOfWork.Documents.GetDocumentForSyncSigefe(documentId);

                if (docToSync.IsProcessed)
                    throw new Exception("Documento já se encontra processado.");

                var setDocFaturacaoResponse = await RequestSetDocFaturacao(docToSync);

                //Caso 1 - Indisponibilidade do serviço
                if (setDocFaturacaoResponse == null || setDocFaturacaoResponse.messages == null)
                    throw new Exception("Erro de comunicação com SIGeFE. Tentar mais tarde.");

                //Caso 2 - Validação bem sucedida... atualizar documento com o objecto resultado
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

                    //2.1 - Processamento de faturas...
                    if (docToSync.TypeId == DocumentTypeEnum.Fatura 
                        || docToSync.TypeId == DocumentTypeEnum.FaturaSimplificada 
                        || docToSync.TypeId == DocumentTypeEnum.FaturaRecibo 
                        || docToSync.TypeId == DocumentTypeEnum.FaturaAdiantamento)
                    {
                        docToSync.IsMEGA = true;
                        docToSync.IsProcessed = true;

                        docToSync.MEId = docToSyncResult.id_me_fatura;

                        if (!string.IsNullOrEmpty(docToSyncResult.num_compromisso))
                            docToSync.CompromiseNumber = docToSyncResult.num_compromisso;

                        docToSync.IsSynchronizedWithFEAP = false;

                        //Caso 2.1.1 - Fatura Válida (id 35)
                        if (docToSyncResult.state_id == "35")
                        {
                            docToSync.StateId = DocumentStateEnum.ValidadoConferido;
                            docToSync.StateDate = DateTime.Now;
                        }

                        //Caso 2.1.2 - Fatura Inválida (id 22)
                        if (docToSyncResult.state_id == "22")
                        {
                            docToSync.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                            docToSync.ActionDate = DateTime.Now;
                        }

                        unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSync.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização ? docToSyncResult.reason : null));

                        unitOfWork.Documents.Update(docToSync);

                        await unitOfWork.SaveChangesAsync();

                        return;
                    }

                    //2.2 - Processamento de nota de crédito...
                    if (docToSync.TypeId == DocumentTypeEnum.NotaCrédito)
                    {
                        //Verificar se não foi encontrada fatura correspondente (status code 490)
                        if (docToSyncResult.cod_msg_fat == "490")
                        {
                            docToSync.IsMEGA = true;
                            docToSync.IsProcessed = false;
                        }
                        else
                        {
                            docToSync.IsMEGA = true;
                            docToSync.IsProcessed = true;

                            docToSync.MEId = docToSyncResult.id_me_fatura;

                            if (!string.IsNullOrEmpty(docToSyncResult.num_compromisso))
                                docToSync.CompromiseNumber = docToSyncResult.num_compromisso;

                            docToSync.IsSynchronizedWithFEAP = false;

                            //Atualizar campos de referencia à fatura...
                            docToSync.RelatedReferenceNumber = docToSyncResult.num_doc_rel;
                            docToSync.RelatedDocumentId = docToSyncResult.id_doc_feap_rel;

                            //Caso 2.2.1 - Nota de Crédito válida (id 35)
                            if (docToSyncResult.state_id == "35")
                            {
                                docToSync.StateId = DocumentStateEnum.Processado;
                                docToSync.StateDate = DateTime.Now;

                                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync));

                                var relatedDocument = await unitOfWork.Documents.Find(x => x.DocumentId == docToSync.RelatedDocumentId);

                                if (relatedDocument != null)
                                {
                                    relatedDocument.StateId = DocumentStateEnum.Processado;
                                    relatedDocument.StateDate = DateTime.Now;
                                    relatedDocument.IsSynchronizedWithFEAP = false;

                                    unitOfWork.Documents.Update(relatedDocument);

                                    unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument));
                                }
                            }

                            //Caso 2.2.2 - Nota de Crédito Inválida (id 22)
                            if (docToSyncResult.state_id == "22")
                            {
                                docToSync.StateId = DocumentStateEnum.Devolvido;
                                docToSync.StateDate = DateTime.Now;

                                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSyncResult.reason));
                            }
                        }

                        unitOfWork.Documents.Update(docToSync);

                        await unitOfWork.SaveChangesAsync();

                        return;
                    }
                }

                //Caso 3 - Restantes respostas...
                //Adicionar message com a resposta...
                foreach (var message in setDocFaturacaoResponse.messages)
                {
                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                    {
                        DocumentId = docToSync.DocumentId,
                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                        Date = DateTime.Now,
                        MessageCode = message.cod_msg,
                        MessageContent = message.msg
                    });
                }

                // 3.1 - Nif inválido (não é fornecedor MEGA,  status code 422) ou fatura ano letivo anterior (status code 410)
                if (setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "422") || setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "410"))
                {
                    docToSync.IsMEGA = false;
                    docToSync.IsProcessed = true;
                }

                unitOfWork.Documents.Update(docToSync);

                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task SyncFeap(string documentId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docToSync = await unitOfWork.Documents.Find(x => x.DocumentId == documentId);

                if (docToSync.IsSynchronizedWithFEAP)
                    throw new Exception("Documento já se encontra sincronizado.");
                if (!docToSync.IsProcessed)
                    throw new Exception("Documento necessita ser processado pelo SIGeFE primeiro.");
                if (!docToSync.IsMEGA)
                    throw new Exception("Documento não referente ao programa MEGA.");

                var lastSigefeMessage = await unitOfWork.DocumentMessages.GetLastSigefeMessage(documentId);

                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, lastSigefeMessage?.MessageContent));

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

                docToReset.IsProcessed = false;
                docToReset.IsMEGA = false;
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

        public async Task DeleteDocument(string documentId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docToDelete = await unitOfWork.Documents.GetDocumentForDelete(documentId);

                unitOfWork.DocumentLines.DeleteRange(docToDelete.DocumentLines);

                unitOfWork.DocumentMessages.DeleteRange(docToDelete.DocumentMessages);

                foreach (var requestLog in docToDelete.RequestLogs)
                {
                    if (requestLog.RequestLogFileId.HasValue)
                    {
                        var requestLogFile = await unitOfWork.RequestLogFiles.Find(x => x.Id == requestLog.RequestLogFileId);
                        unitOfWork.RequestLogFiles.Delete(requestLogFile);
                    }

                }

                unitOfWork.RequestLogs.DeleteRange(docToDelete.RequestLogs);

                unitOfWork.DocumentFiles.Delete(docToDelete.PdfFile);

                unitOfWork.DocumentFiles.Delete(docToDelete.UblFile);

                if (docToDelete.AttachsFileId.HasValue)
                    unitOfWork.DocumentFiles.Delete(docToDelete.AttachsFile);

                unitOfWork.Documents.Delete(docToDelete);

                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<DocumentFile> GetFilesForDownload(int id)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.DocumentFiles.Find(x => x.Id == id);
        }

        #endregion


        #region Homepage

        public async Task<int> GetTotalDocument()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.Documents.Count(x => x.SchoolYear == _anoLetivoAtual);
        }

        public async Task<int> GetTotalDocumentsToSyncFeap()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.Documents.GetDocumentsToSyncFeapCount(_anoLetivoAtual);
        }

        public async Task<int> GetTotalUnprocessedDocument()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.Documents.Count(x => !x.IsMEGA && !x.IsProcessed && x.SchoolYear == _anoLetivoAtual);
        }

        public async Task<int> GetTotalMEGADocument()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.Documents.Count(x => x.SchoolYear == _anoLetivoAtual && x.IsMEGA);
        }

        public async Task<int> GetTotalNotMEGADocument()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.Documents.Count(x => x.SchoolYear == _anoLetivoAtual && !x.IsMEGA && x.IsProcessed);
        }

        public async Task<int> GetTotalMEGADocumentsByType(DocumentTypeEnum[] types, DocumentStateEnum? stateId = null, DocumentActionEnum? actionId = null)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
                return await unitOfWork.Documents.Count(x => x.SchoolYear == _anoLetivoAtual
                                                        && x.IsMEGA
                                                        && types.Contains(x.TypeId)
                                                        && (!actionId.HasValue || x.ActionId == actionId.Value)
                                                        && (!stateId.HasValue || x.StateId == stateId.Value));
        }

        public async Task SyncAllDocumentsFeap()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var errors = new List<string>();

                var docsToSync = await unitOfWork.Documents.GetDocumentsToSyncFeap(_anoLetivoAtual);

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
                return new PaginatedResult<Document>()
                {
                    PageIndex = filters.PageIndex,
                    PageSize = filters.PageSize,
                    TotalCount = await unitOfWork.Documents.GetDocsToSyncSigefeCount(_anoLetivoAtual),
                    Data = await unitOfWork.Documents.GetPaginatedDocsToSyncSigefe(_anoLetivoAtual, filters),
                };
            }
        }

        public async Task SyncDocumentsSigefe(string documentId = null)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docsToSyncSigefe = await unitOfWork.Documents.GetDocsToSyncSigefe(_anoLetivoAtual, documentId);

                var errors = new List<string>();

                foreach (var docToSync in docsToSyncSigefe)
                {
                    try
                    {
                        var setDocFaturacaoResponse = await RequestSetDocFaturacao(docToSync);

                        //Caso 1 - Indisponibilidade do serviço
                        if (setDocFaturacaoResponse == null || setDocFaturacaoResponse.messages == null)
                            throw new Exception("Erro de comunicação com SIGeFE. Tentar mais tarde.");

                        //Caso 2 - Validação bem sucedida... atualizar documento com o objecto resultado
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

                            //2.1 - Processamento de faturas...
                            if (docToSync.TypeId == DocumentTypeEnum.Fatura 
                                || docToSync.TypeId == DocumentTypeEnum.FaturaSimplificada 
                                || docToSync.TypeId == DocumentTypeEnum.FaturaRecibo 
                                || docToSync.TypeId == DocumentTypeEnum.FaturaAdiantamento)
                            {
                                docToSync.IsMEGA = true;
                                docToSync.IsProcessed = true;

                                docToSync.MEId = docToSyncResult.id_me_fatura;

                                if (!string.IsNullOrEmpty(docToSyncResult.num_compromisso))
                                    docToSync.CompromiseNumber = docToSyncResult.num_compromisso;

                                docToSync.IsSynchronizedWithFEAP = false;

                                //Caso 2.1.1 - Fatura Válida (id 35)
                                if (docToSyncResult.state_id == "35")
                                {
                                    docToSync.StateId = DocumentStateEnum.ValidadoConferido;
                                    docToSync.StateDate = DateTime.Now;
                                }

                                //Caso 2.1.2 - Fatura Inválida (id 22)
                                if (docToSyncResult.state_id == "22")
                                {
                                    docToSync.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                    docToSync.ActionDate = DateTime.Now;
                                }

                                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSync.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização ? docToSyncResult.reason : null));

                                unitOfWork.Documents.Update(docToSync);

                                await unitOfWork.SaveChangesAsync();

                                continue;
                            }

                            //2.2 - Processamento de nota de crédito...
                            if (docToSync.TypeId == DocumentTypeEnum.NotaCrédito)
                            {
                                //Verificar se não foi encontrada fatura correspondente(status code 490)
                                if (docToSyncResult.cod_msg_fat == "490")
                                {
                                    docToSync.IsMEGA = true;
                                    docToSync.IsProcessed = false;
                                }
                                else
                                {
                                    docToSync.IsMEGA = true;
                                    docToSync.IsProcessed = true;

                                    docToSync.MEId = docToSyncResult.id_me_fatura;

                                    if (!string.IsNullOrEmpty(docToSyncResult.num_compromisso))
                                        docToSync.CompromiseNumber = docToSyncResult.num_compromisso;

                                    docToSync.IsSynchronizedWithFEAP = false;

                                    //Atualizar campos de referencia à fatura...
                                    docToSync.RelatedReferenceNumber = docToSyncResult.num_doc_rel;
                                    docToSync.RelatedDocumentId = docToSyncResult.id_doc_feap_rel;

                                    //Caso 2.2.1 - Nota de Crédito válida (id 35)
                                    if (docToSyncResult.state_id == "35")
                                    {
                                        docToSync.StateId = DocumentStateEnum.Processado;
                                        docToSync.StateDate = DateTime.Now;

                                        unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync));

                                        var relatedDocument = await unitOfWork.Documents.Find(x => x.DocumentId == docToSync.RelatedDocumentId);

                                        if (relatedDocument != null)
                                        {
                                            relatedDocument.StateId = DocumentStateEnum.Processado;
                                            relatedDocument.StateDate = DateTime.Now;
                                            relatedDocument.IsSynchronizedWithFEAP = false;

                                            unitOfWork.Documents.Update(relatedDocument);

                                            unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument));
                                        }
                                    }

                                    //Caso 2.2.2 - Nota de Crédito Inválida (id 22)
                                    if (docToSyncResult.state_id == "22")
                                    {
                                        docToSync.StateId = DocumentStateEnum.Devolvido;
                                        docToSync.StateDate = DateTime.Now;

                                        unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSyncResult.reason));
                                    }
                                }

                                unitOfWork.Documents.Update(docToSync);

                                await unitOfWork.SaveChangesAsync();

                                continue;
                            }
                        }

                        //Caso 3 - Restantes respostas...
                        //Adicionar message com a resposta...
                        foreach (var message in setDocFaturacaoResponse.messages)
                        {
                            unitOfWork.DocumentMessages.Add(new DocumentMessage()
                            {
                                DocumentId = docToSync.DocumentId,
                                MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                Date = DateTime.Now,
                                MessageCode = message.cod_msg,
                                MessageContent = message.msg
                            });
                        }


                        // 3.1 - Nif inválido (não é fornecedor MEGA,  status code 422) ou fatura ano letivo anterior (status code 410)
                        if (setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "422") || setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "410"))
                        {
                            docToSync.IsMEGA = false;
                            docToSync.IsProcessed = true;
                        }

                        unitOfWork.Documents.Update(docToSync);

                        await unitOfWork.SaveChangesAsync();
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
                var GetDocFaturacaoResponse = await GetDocFaturacao("5");

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
                var GetDocFaturacaoResponse = await GetDocFaturacao("5");

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
                return await unitOfWork.Documents.GetCreditNotesToReprocessCount(_anoLetivoAtual);
            }
        }

        public async Task ReprocessCreditNotes()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docsToReprocess = await unitOfWork.Documents.GetCreditNotesToReprocess(_anoLetivoAtual);

                var errors = new List<string>();

                foreach (var docToReprocess in docsToReprocess)
                {
                    try
                    {
                        var setDocFaturacaoResponse = await RequestSetDocFaturacao(docToReprocess);

                        //Indisponibilidade do serviço
                        if (setDocFaturacaoResponse == null || setDocFaturacaoResponse.messages == null)
                            throw new Exception("Erro de comunicação com SIGeFE. Tentar mais tarde.");

                        if (!setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "200"))
                        {
                            var setDocFaturacaoMessage = setDocFaturacaoResponse.messages.FirstOrDefault();

                            throw new Exception($"{setDocFaturacaoMessage.msg} ({setDocFaturacaoMessage.cod_msg}).");
                        }
                        else
                        {
                            //Validação bem sucedida... atualizar documento com o objecto resultado
                            var docToReprocessResult = setDocFaturacaoResponse.faturas.FirstOrDefault();

                            unitOfWork.DocumentMessages.Add(new DocumentMessage()
                            {
                                DocumentId = docToReprocess.DocumentId,
                                MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                Date = DateTime.Now,
                                MessageCode = docToReprocessResult.cod_msg_fat,
                                MessageContent = docToReprocessResult.msg_fat
                            });

                            docToReprocess.IsMEGA = true;
                            docToReprocess.IsProcessed = true;

                            docToReprocess.MEId = docToReprocessResult.id_me_fatura;

                            if (!string.IsNullOrEmpty(docToReprocessResult.num_compromisso))
                                docToReprocess.CompromiseNumber = docToReprocessResult.num_compromisso;

                            docToReprocess.IsSynchronizedWithFEAP = false;

                            //Atualizar campos de referencia à fatura...
                            docToReprocess.RelatedReferenceNumber = docToReprocessResult.num_doc_rel;
                            docToReprocess.RelatedDocumentId = docToReprocessResult.id_doc_feap_rel;

                            //Nota de Crédito válida (id 35)
                            if (docToReprocessResult.state_id == "35")
                            {
                                docToReprocess.StateId = DocumentStateEnum.Processado;
                                docToReprocess.StateDate = DateTime.Now;

                                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToReprocess));

                                var relatedDocument = await unitOfWork.Documents.Find(x => x.DocumentId == docToReprocess.RelatedDocumentId);

                                if (relatedDocument != null)
                                {
                                    relatedDocument.StateId = DocumentStateEnum.Processado;
                                    relatedDocument.StateDate = DateTime.Now;
                                    relatedDocument.IsSynchronizedWithFEAP = false;

                                    unitOfWork.Documents.Update(relatedDocument);

                                    unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument));
                                }
                            }

                            //Nota de Crédito Inválida (id 22)
                            if (docToReprocessResult.state_id == "22" || docToReprocessResult.cod_msg_fat == "490")
                            {
                                docToReprocess.StateId = DocumentStateEnum.Devolvido;
                                docToReprocess.StateDate = DateTime.Now;

                                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToReprocess, docToReprocessResult.reason));
                            }

                            unitOfWork.Documents.Update(docToReprocess);

                            await unitOfWork.SaveChangesAsync();
                        }
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

        #endregion


        #region Private Methods

        private async Task<GetDocFaturacaoResponse> GetDocFaturacao(string estado_doc, string nif = null, string id_doc_feap = null)
        {
            var headers = new Dictionary<string, string>
            {
                { "id_ano_letivo", _anoLetivoAtual },
                { "estado_doc", estado_doc }
            };

            if (!string.IsNullOrEmpty(nif))
                headers.Add("nif", nif);

            if (!string.IsNullOrEmpty(id_doc_feap))
                headers.Add("id_doc_feap", id_doc_feap);

            return await _genericRestRequestManager.Get<GetDocFaturacaoResponse>("getDocFaturacao", headers);
        }

        private async Task<RequestLog> RequestSetDocument(Document document, string reason = null)
        {
            var uniqueId = Guid.NewGuid();

            var requestLog = new RequestLog
            {
                UniqueId = uniqueId,
                RequestLogTypeId = RequestLogTypeEnum.SetDocument,
                DocumentId = document.DocumentId,
                SupplierFiscalId = document.SupplierFiscalId,
                ReferenceNumber = document.ReferenceNumber,
                Date = DateTime.Now
            };

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
                        ProcessId = document.DocumentId,
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
                                    case DocumentTypeEnum.FaturaAdiantamento:
                                    case DocumentTypeEnum.FaturaSimplificada:
                                    case DocumentTypeEnum.FaturaRecibo:
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

                    requestLog.Successful = true;
                }
            }
            catch (Exception ex)
            {
                var stackTrace = new StackTrace(ex, fNeedFileInfo: true);
                var firstFrame = stackTrace.FrameCount > 0 ? stackTrace.GetFrame(0) : null;

                requestLog.Successful = false;
                requestLog.ExceptionType = ex.GetBaseException().GetType().Name;
                requestLog.ExceptionAtFile = firstFrame?.GetFileName();
                requestLog.ExceptionAtLine = firstFrame?.GetFileLineNumber();
                requestLog.ExceptionMessage = ex.GetBaseException().Message;
            }

            if (FileManager.FileExists(requestLog.RequestLogTypeId.ToString(), uniqueId.ToString()))
            {
                requestLog.RequestLogFile = new RequestLogFile
                {
                    Content = FileManager.GetFile(requestLog.RequestLogTypeId.ToString(), uniqueId.ToString()),
                    ContentType = "application/xml"
                };
            }

            return requestLog;
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
                fatura_base64 = Convert.ToBase64String(document.PdfFile.Content),
                dt_fatura = document.IssueDate.ToString("dd-MM-yyyy"),
                num_compromisso = document.CompromiseNumber,
            };

            switch (document.TypeId)
            {
                case DocumentTypeEnum.Fatura:
                case DocumentTypeEnum.FaturaSimplificada:
                case DocumentTypeEnum.FaturaRecibo:
                case DocumentTypeEnum.FaturaAdiantamento:
                    faturaToSend.tp_doc = "FAT";
                    break;
                case DocumentTypeEnum.NotaCrédito:
                    faturaToSend.tp_doc = "NTC";
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
