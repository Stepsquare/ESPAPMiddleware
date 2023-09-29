using EspapMiddleware.ServiceLayer.FEAPServices_PP;
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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static EspapMiddleware.Shared.WebServiceModels.GetDocFaturacaoResponse;

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
                                        && (string.IsNullOrEmpty(filters.CompromiseNumber) || x.CompromiseNumber == filters.CompromiseNumber)
                                        && (string.IsNullOrEmpty(filters.ReferenceNumber) || x.ReferenceNumber == filters.ReferenceNumber)
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

                var docToSyncResult = await RequestSetDocFaturacao(docToSync);

                unitOfWork.DocumentMessages.Add(new DocumentMessage()
                {
                    DocumentId = docToSync.DocumentId,
                    MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                    Date = DateTime.Now,
                    MessageCode = docToSyncResult != null ? docToSyncResult.cod_msg_fat : "500",
                    MessageContent = docToSyncResult != null ? docToSyncResult.msg_fat : "Falha de comunicação. Reenviar pedido mais tarde."
                });

                if (docToSyncResult == null)
                {
                    throw new Exception("Erro de comunicação com SIGeFE. Tentar mais tarde");
                }
                else
                {
                    if (docToSync.TypeId == DocumentTypeEnum.Fatura)
                    {
                        if (docToSync.StateId == DocumentStateEnum.EmitidoPagamento)
                        {
                            docToSync.IsSynchronizedWithSigefe = docToSyncResult != null && docToSyncResult.cod_msg_fat == "200";
                        }
                        else
                        {
                            docToSync.MEId = docToSyncResult.id_me_fatura;

                            docToSync.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(docToSyncResult.id_me_fatura);
                            docToSync.IsSynchronizedWithFEAP = !docToSync.IsSynchronizedWithSigefe;

                            if (docToSyncResult.state_id == "35")
                            {
                                docToSync.StateId = DocumentStateEnum.ValidadoConferido;
                                docToSync.StateDate = DateTime.Now;
                            }
                            else
                            {
                                docToSync.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                docToSync.ActionDate = DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        if (docToSyncResult.cod_msg_fat == "490")
                        {
                            throw new Exception("Sincronização falhada. Sincronizar primeiro fatura associada.");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(docToSync.RelatedDocumentId))
                            {
                                var relatedDocument = await unitOfWork.Documents.GetRelatedDocument(docToSync.RelatedReferenceNumber,
                                                                                                docToSync.SupplierFiscalId,
                                                                                                docToSync.SchoolYear,
                                                                                                docToSync.TypeId);

                                if (relatedDocument != null)
                                {
                                    docToSync.RelatedDocumentId = relatedDocument.DocumentId;
                                    docToSync.RelatedDocument = relatedDocument;
                                }
                            }

                            docToSync.MEId = docToSyncResult.id_me_fatura;

                            docToSync.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(docToSyncResult.id_me_fatura);
                            docToSync.IsSynchronizedWithFEAP = !docToSync.IsSynchronizedWithSigefe;

                            if (docToSyncResult.state_id == "35")
                            {
                                docToSync.StateId = DocumentStateEnum.Processado;
                                docToSync.StateDate = DateTime.Now;

                                docToSync.RelatedDocument.StateId = DocumentStateEnum.Processado;
                                docToSync.RelatedDocument.StateDate = DateTime.Now;
                                docToSync.RelatedDocument.IsSynchronizedWithFEAP = docToSync.IsSynchronizedWithFEAP;
                            }
                            else
                            {
                                docToSync.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                docToSync.ActionDate = DateTime.Now;
                            }
                        }
                    }
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

                var setDocumentLog = await RequestSetDocument(docToSync, lastSigefeMessage?.MessageContent);

                unitOfWork.RequestLogs.Add(setDocumentLog);

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro ao inserir Log de comunicação na BD.");
                }
            }
        }

        public async Task ReturnDocument(string documentId, string reason)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docToReturn = await unitOfWork.Documents.Find(x => x.DocumentId == documentId);

                if (docToReturn.StateId != DocumentStateEnum.Iniciado)
                    throw new Exception("Não é possivél devolver o documento.");

                docToReturn.StateId = DocumentStateEnum.Devolvido;
                docToReturn.StateDate = DateTime.Now;

                var setDocumentLog = await RequestSetDocument(docToReturn, reason);

                unitOfWork.RequestLogs.Add(setDocumentLog);

                docToReturn.IsSynchronizedWithFEAP = setDocumentLog.Successful;

                unitOfWork.Documents.Update(docToReturn);

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro ao inserir Log de comunicação na BD.");
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

                unitOfWork.Documents.Update(docToReset);

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro ao inserir Log de comunicação na BD.");
                }
            }
        }

        #endregion


        #region Homepage

        public async Task<(int totalDocuments, int totalDocumentsNotSyncFeap, int totalValidDocuments, int totalValidDocumentsNotSyncFeap, int totalInvalidDocuments, int totalInvalidDocumentsNotSyncFeap, int totalInvalidDocumentsRectified, int totalInvalidDocumentsRectifiedNotSyncFeap, int totalPaidDocuments, int totalPaidDocumentsNotSyncFeap)> GetGlobalStatus(string anoLetivo)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var totalDocuments = await unitOfWork.Documents.Count(x => x.SchoolYear == anoLetivo);

                var totalDocumentsNotSyncFeap = await unitOfWork.Documents.Count(x => x.SchoolYear == anoLetivo && !x.IsSynchronizedWithFEAP);


                var totalValidDocuments = await unitOfWork.Documents.Count(x => x.StateId == DocumentStateEnum.ValidadoConferido && x.SchoolYear == anoLetivo);

                var totalValidDocumentsNotSyncFeap = await unitOfWork.Documents.Count(x => x.StateId == DocumentStateEnum.ValidadoConferido && x.SchoolYear == anoLetivo && !x.IsSynchronizedWithFEAP);


                var totalInvalidDocuments = await unitOfWork.Documents.Count(x => x.StateId == DocumentStateEnum.Iniciado
                                                                               && x.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização
                                                                               && x.SchoolYear == anoLetivo);

                var totalInvalidDocumentsNotSyncFeap = await unitOfWork.Documents.Count(x => x.StateId == DocumentStateEnum.Iniciado
                                                                               && x.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização
                                                                               && x.SchoolYear == anoLetivo
                                                                               && !x.IsSynchronizedWithFEAP);


                var totalInvalidDocumentsRectified = await unitOfWork.Documents.Count(x => x.StateId == DocumentStateEnum.Processado
                                                                               && x.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização
                                                                               && x.SchoolYear == anoLetivo);

                var totalInvalidDocumentsRectifiedNotSyncFeap = await unitOfWork.Documents.Count(x => x.StateId == DocumentStateEnum.Processado
                                                                               && x.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização
                                                                               && x.SchoolYear == anoLetivo
                                                                               && !x.IsSynchronizedWithFEAP);


                var totalPaidDocuments = await unitOfWork.Documents.Count(x => x.StateId == DocumentStateEnum.EmitidoPagamento
                                                                            && x.SchoolYear == anoLetivo);

                var totalPaidDocumentsNotSyncFeap = await unitOfWork.Documents.Count(x => x.StateId == DocumentStateEnum.EmitidoPagamento
                                                                            && x.SchoolYear == anoLetivo
                                                                            && !x.IsSynchronizedWithFEAP);

                return (totalDocuments, totalDocumentsNotSyncFeap, totalValidDocuments, totalValidDocumentsNotSyncFeap, totalInvalidDocuments, totalInvalidDocumentsNotSyncFeap, totalInvalidDocumentsRectified, totalInvalidDocumentsRectifiedNotSyncFeap, totalPaidDocuments, totalPaidDocumentsNotSyncFeap);
            }
        }

        public async Task SyncAllDocumentsFeap(string anoLetivo, DocumentStateEnum? stateId, DocumentActionEnum? actionId = null)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var errors = new List<string>();

                var docsToSync = await unitOfWork.Documents.GetDocumentsToSyncFeap(anoLetivo, stateId, actionId);

                foreach (var doc in docsToSync)
                {
                    try
                    {
                        var lastSigefeMessage = await unitOfWork.DocumentMessages.GetLastSigefeMessage(doc.DocumentId);

                        var setDocumentRquestLog = await RequestSetDocument(doc, lastSigefeMessage?.MessageContent);

                        unitOfWork.RequestLogs.Add(setDocumentRquestLog);

                        if (!setDocumentRquestLog.Successful)
                        {
                            throw new Exception("Falha de comunicação com FEAP.");
                        }
                        else
                        {
                            doc.IsSynchronizedWithFEAP = false;

                            unitOfWork.Documents.Update(doc);

                            await unitOfWork.SaveChangesAsync();
                        }
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

                if (!string.IsNullOrEmpty(documentId))
                {
                    var docToSync = await unitOfWork.Documents.GetDocumentForSyncSigefe(documentId);

                    var docToSyncResult = await RequestSetDocFaturacao(docToSync);

                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                    {
                        DocumentId = docToSync.DocumentId,
                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                        Date = DateTime.Now,
                        MessageCode = docToSyncResult != null ? docToSyncResult.cod_msg_fat : "500",
                        MessageContent = docToSyncResult != null ? docToSyncResult.msg_fat : "Falha de comunicação. Reenviar pedido mais tarde."
                    });

                    if (docToSyncResult != null)
                    {
                        if (docToSync.TypeId == DocumentTypeEnum.Fatura)
                        {
                            docToSync.MEId = docToSyncResult.id_me_fatura;

                            docToSync.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(docToSyncResult.id_me_fatura);
                            docToSync.IsSynchronizedWithFEAP = !docToSync.IsSynchronizedWithSigefe;

                            if (docToSyncResult.state_id == "35")
                            {
                                docToSync.StateId = DocumentStateEnum.ValidadoConferido;
                                docToSync.StateDate = DateTime.Now;

                                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync));
                            }
                            else
                            {
                                var relatedDocument = await unitOfWork.Documents.GetRelatedDocument(docToSync.RelatedReferenceNumber,
                                                                                                    docToSync.SupplierFiscalId,
                                                                                                    docToSync.SchoolYear,
                                                                                                    docToSync.TypeId);

                                if (relatedDocument != null)
                                {
                                    relatedDocument.RelatedDocumentId = docToSync.DocumentId;

                                    var relatedSetDocFaturacaoResult = await RequestSetDocFaturacao(relatedDocument);

                                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                                    {
                                        DocumentId = relatedDocument.DocumentId,
                                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                        Date = DateTime.Now,
                                        MessageCode = relatedSetDocFaturacaoResult != null ? relatedSetDocFaturacaoResult.cod_msg_fat : "500",
                                        MessageContent = relatedSetDocFaturacaoResult != null ? relatedSetDocFaturacaoResult.msg_fat : "Falha de comunicação. Reenviar pedido mais tarde."
                                    });

                                    if (relatedSetDocFaturacaoResult != null)
                                    {
                                        relatedDocument.MEId = relatedSetDocFaturacaoResult.id_me_fatura;

                                        relatedDocument.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(relatedSetDocFaturacaoResult.id_me_fatura);
                                        relatedDocument.IsSynchronizedWithFEAP = !relatedDocument.IsSynchronizedWithSigefe;

                                        if (relatedSetDocFaturacaoResult.state_id == "35")
                                        {
                                            docToSync.StateId = DocumentStateEnum.Processado;
                                            docToSync.StateDate = DateTime.Now;
                                            unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync));

                                            relatedDocument.StateId = DocumentStateEnum.Processado;
                                            relatedDocument.StateDate = DateTime.Now;
                                            unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument));
                                        }
                                        else
                                        {
                                            relatedDocument.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                            relatedDocument.ActionDate = DateTime.Now;

                                            unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument, relatedSetDocFaturacaoResult.reason));
                                        }

                                        unitOfWork.Documents.Update(relatedDocument);
                                    }
                                }
                                else
                                {
                                    docToSync.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                    docToSync.ActionDate = DateTime.Now;

                                    unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSyncResult.msg_fat));
                                }
                            }
                        }
                        else
                        {
                            if (docToSyncResult.cod_msg_fat != "490")
                            {
                                if (string.IsNullOrEmpty(docToSync.RelatedDocumentId))
                                {
                                    var relatedDocument = await unitOfWork.Documents.GetRelatedDocument(docToSync.RelatedReferenceNumber,
                                                                                                    docToSync.SupplierFiscalId,
                                                                                                    docToSync.SchoolYear,
                                                                                                    docToSync.TypeId);

                                    if (relatedDocument != null)
                                    {
                                        docToSync.RelatedDocumentId = relatedDocument.DocumentId;
                                        docToSync.RelatedDocument = relatedDocument;
                                    }
                                }

                                docToSync.MEId = docToSyncResult.id_me_fatura;

                                docToSync.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(docToSyncResult.id_me_fatura);
                                docToSync.IsSynchronizedWithFEAP = !docToSync.IsSynchronizedWithSigefe;

                                if (docToSyncResult.state_id == "35")
                                {
                                    docToSync.StateId = DocumentStateEnum.Processado;
                                    docToSync.StateDate = DateTime.Now;

                                    docToSync.RelatedDocument.StateId = DocumentStateEnum.Processado;
                                    docToSync.RelatedDocument.StateDate = DateTime.Now;
                                    docToSync.RelatedDocument.IsSynchronizedWithFEAP = docToSync.IsSynchronizedWithFEAP;

                                    unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync.RelatedDocument, docToSyncResult.msg_fat));
                                }
                                else
                                {
                                    docToSync.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                    docToSync.ActionDate = DateTime.Now;
                                }

                                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSyncResult.msg_fat));
                            }
                        }

                        unitOfWork.Documents.Update(docToSync);
                    }

                    var result = await unitOfWork.SaveChangesAsync();

                    if (result == 0)
                        throw new DatabaseException("Erro ao atualizar documento na BD.");
                }
                else
                {
                    var docsToSyncSigefe = await unitOfWork.Documents.GetDocsToSyncSigefe(getFaseResponse.id_ano_letivo_atual);

                    foreach (var docToSync in docsToSyncSigefe)
                    {
                        var docToSyncResult = await RequestSetDocFaturacao(docToSync);

                        unitOfWork.DocumentMessages.Add(new DocumentMessage()
                        {
                            DocumentId = docToSync.DocumentId,
                            MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                            Date = DateTime.Now,
                            MessageCode = docToSyncResult != null ? docToSyncResult.cod_msg_fat : "500",
                            MessageContent = docToSyncResult != null ? docToSyncResult.msg_fat : "Falha de comunicação. Reenviar pedido mais tarde."
                        });

                        if (docToSyncResult != null)
                        {
                            if (docToSync.TypeId == DocumentTypeEnum.Fatura)
                            {
                                docToSync.MEId = docToSyncResult.id_me_fatura;

                                docToSync.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(docToSyncResult.id_me_fatura);
                                docToSync.IsSynchronizedWithFEAP = !docToSync.IsSynchronizedWithSigefe;

                                if (docToSyncResult.state_id == "35")
                                {
                                    docToSync.StateId = DocumentStateEnum.ValidadoConferido;
                                    docToSync.StateDate = DateTime.Now;

                                    unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync));
                                }
                                else
                                {
                                    var relatedDocument = await unitOfWork.Documents.GetRelatedDocument(docToSync.RelatedReferenceNumber,
                                                                                                    docToSync.SupplierFiscalId,
                                                                                                    docToSync.SchoolYear,
                                                                                                    docToSync.TypeId);

                                    if (relatedDocument != null)
                                    {
                                        relatedDocument.RelatedDocumentId = docToSync.DocumentId;

                                        var relatedSetDocFaturacaoResult = await RequestSetDocFaturacao(relatedDocument);

                                        unitOfWork.DocumentMessages.Add(new DocumentMessage()
                                        {
                                            DocumentId = relatedDocument.DocumentId,
                                            MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                            Date = DateTime.Now,
                                            MessageCode = relatedSetDocFaturacaoResult != null ? relatedSetDocFaturacaoResult.cod_msg_fat : "500",
                                            MessageContent = relatedSetDocFaturacaoResult != null ? relatedSetDocFaturacaoResult.msg_fat : "Falha de comunicação. Reenviar pedido mais tarde."
                                        });

                                        if (relatedSetDocFaturacaoResult != null)
                                        {
                                            relatedDocument.MEId = relatedSetDocFaturacaoResult.id_me_fatura;

                                            relatedDocument.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(relatedSetDocFaturacaoResult.id_me_fatura);
                                            relatedDocument.IsSynchronizedWithFEAP = !relatedDocument.IsSynchronizedWithSigefe;

                                            if (relatedSetDocFaturacaoResult.state_id == "35")
                                            {
                                                docToSync.StateId = DocumentStateEnum.Processado;
                                                docToSync.StateDate = DateTime.Now;
                                                unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync));

                                                relatedDocument.StateId = DocumentStateEnum.Processado;
                                                relatedDocument.StateDate = DateTime.Now;
                                                unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument));
                                            }
                                            else
                                            {
                                                relatedDocument.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                                relatedDocument.ActionDate = DateTime.Now;

                                                unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument, relatedSetDocFaturacaoResult.reason));
                                            }

                                            unitOfWork.Documents.Update(relatedDocument);
                                        }
                                    }
                                    else
                                    {
                                        docToSync.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                        docToSync.ActionDate = DateTime.Now;

                                        unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSyncResult.msg_fat));
                                    }
                                }
                            }
                            else
                            {
                                if (docToSyncResult.cod_msg_fat != "490")
                                {
                                    if (string.IsNullOrEmpty(docToSync.RelatedDocumentId))
                                    {
                                        var relatedDocument = await unitOfWork.Documents.GetRelatedDocument(docToSync.RelatedReferenceNumber,
                                                                                                        docToSync.SupplierFiscalId,
                                                                                                        docToSync.SchoolYear,
                                                                                                        docToSync.TypeId);

                                        if (relatedDocument != null)
                                        {
                                            docToSync.RelatedDocumentId = relatedDocument.DocumentId;
                                            docToSync.RelatedDocument = relatedDocument;
                                        }
                                    }

                                    docToSync.MEId = docToSyncResult.id_me_fatura;

                                    docToSync.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(docToSyncResult.id_me_fatura);
                                    docToSync.IsSynchronizedWithFEAP = !docToSync.IsSynchronizedWithSigefe;

                                    if (docToSyncResult.state_id == "35")
                                    {
                                        docToSync.StateId = DocumentStateEnum.Processado;
                                        docToSync.StateDate = DateTime.Now;

                                        docToSync.RelatedDocument.StateId = DocumentStateEnum.Processado;
                                        docToSync.RelatedDocument.StateDate = DateTime.Now;
                                        docToSync.RelatedDocument.IsSynchronizedWithFEAP = docToSync.IsSynchronizedWithFEAP;

                                        var setDocumentLogRelatedDocument = await RequestSetDocument(docToSync.RelatedDocument, docToSyncResult.msg_fat);

                                        unitOfWork.RequestLogs.Add(setDocumentLogRelatedDocument);
                                    }
                                    else
                                    {
                                        docToSync.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                        docToSync.ActionDate = DateTime.Now;
                                    }

                                    unitOfWork.RequestLogs.Add(await RequestSetDocument(docToSync, docToSyncResult.msg_fat));
                                }
                            }

                            unitOfWork.Documents.Update(docToSync);

                            var result = await unitOfWork.SaveChangesAsync();

                            if (result == 0)
                                throw new DatabaseException("Erro ao atualizar documento na BD.");
                        }
                    }
                }
            }
        }

        public async Task<PaginatedResult<string>> GetPaidDocsToSync(PaginatedSearchFilter filters)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var GetDocFaturacaoResponse = await GetDocFaturacao("4");

                if (GetDocFaturacaoResponse == null)
                    throw new WebserviceException("Erro na chamada ao serviço de Faturação do SIGeFE.");

                var documentIds = GetDocFaturacaoResponse.documentos.Select(x => x.id_doc_feap).ToArray();

                return new PaginatedResult<string>()
                {
                    PageIndex = filters.PageIndex,
                    PageSize = filters.PageSize,
                    TotalCount = await unitOfWork.Documents.GetPaidDocsToSyncCount(documentIds),
                    Data = await unitOfWork.Documents.GetPaginatedPaidDocsToSync(documentIds, filters)
                };
            }
        }

        public async Task SyncPaidDocuments(string documentId = null)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var GetDocFaturacaoResponse = await GetDocFaturacao("4");

                if (GetDocFaturacaoResponse == null)
                    throw new WebserviceException("Erro na chamada ao serviço de Faturação do SIGeFE.");

                if (documentId != null)
                {
                    if (!GetDocFaturacaoResponse.documentos.Any(x => x.id_doc_feap == documentId))
                        throw new Exception("Documento não contemplado na lista de faturas pagas do SIGeFE.");

                    var docToSync = await unitOfWork.Documents.Find(x => x.DocumentId == documentId
                                                            && x.StateId != DocumentStateEnum.EmitidoPagamento);

                    if (docToSync != null)
                    {
                        var setDocumentRquestLog = await RequestSetDocumentPaid(docToSync);

                        unitOfWork.RequestLogs.Add(setDocumentRquestLog);

                        if (setDocumentRquestLog.Successful)
                        {
                            docToSync.StateId = DocumentStateEnum.EmitidoPagamento;
                            docToSync.StateDate = DateTime.Now;
                            docToSync.IsSynchronizedWithFEAP = false;

                            unitOfWork.Documents.Update(docToSync);
                        }

                        await unitOfWork.SaveChangesAsync();

                        if (!setDocumentRquestLog.Successful)
                        {
                            throw new Exception("Falha de comunicação com FEAP.");
                        }
                    }
                    else
                    {
                        throw new Exception("Documento já sincronizado com FE-AP.");
                    }
                }
                else
                {
                    var errors = new List<string>();

                    var documentIds = GetDocFaturacaoResponse.documentos.Select(x => x.id_doc_feap).ToArray();

                    var paidDocsToSync = await unitOfWork.Documents.GetPaidDocsToSync(documentIds);

                    foreach (var document in paidDocsToSync)
                    {
                        try
                        {
                            var setDocumentRquestLog = await RequestSetDocumentPaid(document);

                            unitOfWork.RequestLogs.Add(setDocumentRquestLog);

                            if (setDocumentRquestLog.Successful)
                            {
                                document.StateId = DocumentStateEnum.EmitidoPagamento;
                                document.StateDate = DateTime.Now;
                                document.IsSynchronizedWithFEAP = false;


                                unitOfWork.Documents.Update(document);
                            }

                            await unitOfWork.SaveChangesAsync();

                            if (!setDocumentRquestLog.Successful)
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

        private async Task<RequestLog> RequestSetDocumentPaid(Document document)
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
                        ProcessId = document.DocumentId,
                    };

                    var setDocumentRequest = new FEAPServices_PP.SetDocumentRequest()
                    {
                        uniqueId = uniqueId.ToString(),
                        documentId = document.DocumentId,
                    };

                    setDocumentRequest.stateIdSpecified = true;
                    setDocumentRequest.stateId = (int)DocumentStateEnum.EmitidoPagamento;

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
                    Date = DateTime.Now,
                    SupplierFiscalId = document.SupplierFiscalId,
                    ReferenceNumber = document.ReferenceNumber,
                    Successful = false,
                    ExceptionType = ex.GetBaseException().GetType().Name,
                    ExceptionStackTrace = ex.GetBaseException().StackTrace,
                    ExceptionMessage = ex.GetBaseException().Message
                };
            }
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
                        ProcessId = document.DocumentId,
                    };

                    var setDocumentRequest = new FEAPServices_PP.SetDocumentRequest()
                    {
                        uniqueId = uniqueId.ToString(),
                        documentId = document.DocumentId,
                    };

                    if (document.StateId == DocumentStateEnum.ValidadoConferido || document.StateId == DocumentStateEnum.Processado || document.StateId == DocumentStateEnum.Devolvido)
                    {
                        setDocumentRequest.documentNumbersSpecified1Specified = true;
                        setDocumentRequest.documentNumbersSpecified1 = true;
                        setDocumentRequest.documentNumbers = new string[] { document.MEId };
                        setDocumentRequest.stateIdSpecified = true;
                        setDocumentRequest.stateId = (int)document.StateId;

                        if (document.StateId == DocumentStateEnum.Devolvido)
                            setDocumentRequest.reason = reason;

                        if (document.StateId == DocumentStateEnum.Processado)
                        {
                            setDocumentRequest.commitmentSpecified1Specified = true;
                            setDocumentRequest.commitmentSpecified1 = true;
                            setDocumentRequest.commitment = !string.IsNullOrEmpty(document.CompromiseNumber) ? document.CompromiseNumber : "N/A";

                            setDocumentRequest.postingDateSpecified1Specified = true;
                            setDocumentRequest.postingDateSpecified1 = true;
                            setDocumentRequest.postingDateSpecified = true;
                            setDocumentRequest.postingDate = DateTime.Now;
                        }
                    }
                    else if (document.StateId == DocumentStateEnum.EmitidoPagamento)
                    {
                        setDocumentRequest.stateIdSpecified = true;
                        setDocumentRequest.stateId = (int)DocumentStateEnum.EmitidoPagamento;

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
                    }
                    else if (document.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização)
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

        private async Task<SetDocFaturacaoResponse.fatura> RequestSetDocFaturacao(Document document)
        {
            var setDocFaturacaoObj = new SetDocFaturacao();

            FillWithContract(in document, ref setDocFaturacaoObj);

            var setDocFaturacaoResult = await _genericRestRequestManager.Post<SetDocFaturacaoResponse, SetDocFaturacao>("setDocFaturacao", setDocFaturacaoObj);

            if (setDocFaturacaoResult == null)
                return null;

            if (setDocFaturacaoResult.messages.Any(x => x.cod_msg == "200"))
            {
                return setDocFaturacaoResult.faturas.FirstOrDefault();
            }
            else
            {
                throw new WebserviceException(setDocFaturacaoResult.messages.Select(x => x.msg).ToArray());
            }
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
