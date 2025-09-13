using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.MonitorServiceModels;
using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using EspapMiddleware.Shared.WebServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IServices
{
    public interface IMonitorServices
    {
        Task<GetFaseResponse> GetFase();

        #region RequestLogs

        Task<PaginatedResult<RequestLog>> RequestLogSearch(RequestLogSearchFilters filters);
        Task<RequestLogFile> GetRequestLogFile(int id);

        #endregion

        #region Documents

        Task<PaginatedResult<Document>> DocumentSearch(DocumentSearchFilters filters);
        Task<Document> GetDocumentDetail(string documentId);
        Task<PaginatedResult<DocumentLine>> GetDocumentLinesForDetail(DocumentDetailLineFilter filters);
        Task SyncSigefe(string documentId);
        Task SyncFeap(string documentId);
        Task ResetSigefeSync(string documentId);
        Task ReturnDocument(string documentId, string reason);
        Task DeleteDocument(string documentId);
        Task<DocumentFile> GetFilesForDownload(int id);

        #endregion

        #region Homepage

        Task<int> GetTotalDocument();
        Task<int> GetTotalDocumentsToSyncFeap();
        Task<int> GetTotalUnprocessedDocument();
        Task<int> GetTotalMEGADocument();
        Task<int> GetTotalNotMEGADocument();
        Task<int> GetTotalMEGADocumentsByType(DocumentTypeEnum[] types, DocumentStateEnum? stateId = null, DocumentActionEnum? actionId = null);
        Task SyncAllDocumentsFeap();
        Task<PaginatedResult<Document>> GetDocsToSyncSigefe(PaginatedSearchFilter filters);
        Task SyncDocumentsSigefe(string documentId = null);
        Task<int> GetTotalPaidDocsToSync();
        Task SyncPaidDocuments();
        Task<int> GetTotalCreditNotesToReprocess();
        Task ReprocessCreditNotes();

        #endregion
    }
}
