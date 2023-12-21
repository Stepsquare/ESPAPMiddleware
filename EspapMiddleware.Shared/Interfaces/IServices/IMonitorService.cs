using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.MonitorServiceModels;
using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IServices
{
    public interface IMonitorService
    {
        #region RequestLogs
        Task<PaginatedResult<RequestLog>> RequestLogSearch(RequestLogSearchFilters filters);
        Task<RequestLog> GetLogForDownload(Guid uniqueId, RequestLogTypeEnum type);

        #endregion

        #region Documents

        Task<PaginatedResult<Document>> DocumentSearch(DocumentSearchFilters filters);
        Task<Document> GetDocumentDetail(string documentId);
        Task<PaginatedResult<DocumentLine>> GetDocumentLinesForDetail(DocumentDetailLineFilter filters);
        Task<IEnumerable<string>> GetSchoolYears();
        Task SyncSigefe(string documentId);
        Task SyncFeap(string documentId);
        Task ReturnDocument(string documentId, string reason);
        Task ResetCompromiseNumber(string documentId);
        Task ResetSigefeSync(string documentId);

        #endregion

        #region Homepage

        Task<string> GetCurrentSchoolYear();
        Task<int> GetTotalDocument(string anoLetivo, bool? isSynchronizedWithFEAP = null);
        Task<int> GetTotalDocumentsByType(string anoLetivo, DocumentTypeEnum typeId, DocumentStateEnum? stateId = null, DocumentActionEnum? actionId = null);
        Task SyncAllDocumentsFeap(string anoLetivo);
        Task<PaginatedResult<Document>> GetDocsToSyncSigefe(PaginatedSearchFilter filters);
        Task SyncDocumentsSigefe(string documentId = null);
        Task<int> GetTotalPaidDocsToSync();
        Task SyncPaidDocuments();
        Task<int> GetTotalCreditNotesToReprocess();
        Task ReprocessCreditNotes();
        Task ReturnDebitNotes();

        #endregion
    }
}
