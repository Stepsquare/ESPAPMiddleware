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
        Task ReturnDocument(string documentId, string reason);
        Task ResetSigefeSync(string documentId);
        Task<DocumentFile> GetFilesForDownload(int id);

        #endregion

        #region Homepage

        Task<int> GetTotalDocument(string anoLetivo);
        Task<int> GetTotalDocumentsToSyncFeap(string anoLetivo);
        Task<int> GetTotalUnprocessedDocument(string anoLetivo);
        Task<int> GetTotalMEGADocument(string anoLetivo);
        Task<int> GetTotalNotMEGADocument(string anoLetivo);
        Task<int> GetTotalMEGADocumentsByType(string anoLetivo, DocumentTypeEnum typeId, DocumentStateEnum? stateId = null, DocumentActionEnum? actionId = null);
        Task SyncAllDocumentsFeap(string anoLetivo);
        Task<PaginatedResult<Document>> GetDocsToSyncSigefe(PaginatedSearchFilter filters);
        Task SyncDocumentsSigefe(string documentId = null);
        Task<int> GetTotalPaidDocsToSync();
        Task SyncPaidDocuments();
        Task<int> GetTotalCreditNotesToReprocess();
        Task ReprocessCreditNotes();

        #endregion
    }
}
