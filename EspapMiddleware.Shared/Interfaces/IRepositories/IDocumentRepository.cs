using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.MonitorServiceModels;
using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IRepositories
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        Task<Document> GetDocumentForSyncSigefe(string documentId);
        Task<Document> GetRelatedDocument(string referenceNumber, string supplierFiscalId, string schoolYear, DocumentTypeEnum type);
        Task<Document> GetDocumentForDetail(string documentId);
        Task<IEnumerable<Document>> GetFilteredPaginated(DocumentSearchFilters filters);
        Task<IEnumerable<string>> GetSchoolYears();
        Task<IEnumerable<Document>> GetDocumentsToSyncFeap(string anoLetivo, DocumentStateEnum? stateId, DocumentActionEnum? actionId = null);
        Task<IEnumerable<string>> GetPaginatedPaidDocsToSync(string[] documentIds, PaginatedSearchFilter filters);
        Task<IEnumerable<Document>> GetPaidDocsToSync(string[] documentIds);
        Task<int> GetPaidDocsToSyncCount(string[] documentIds);
        Task<IEnumerable<Document>> GetPaginatedDocsToSyncSigefe(string anoLetivo, PaginatedSearchFilter filters);
        Task<IEnumerable<Document>> GetDocsToSyncSigefe(string anoLetivo);
        Task<int> GetDocsToSyncSigefeCount(string anoLetivo);
    }
}
