﻿using EspapMiddleware.Shared.Entities;
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

        #endregion

        #region Homepage

        Task<(int totalDocuments, int totalValidDocuments, int totalInvalidDocuments, int totalInvalidDocumentsRectified, int totalPaidDocuments)> GetGlobalStatus(string anoLetivo);
        Task<PaginatedResult<string>> GetPaidDocsToSync(PaginatedSearchFilter filters);
        Task SyncPaidDocuments(string documentId = null);

        #endregion
    }
}
