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
        Task<Document> GetDocumentForDelete(string documentId);
        Task<Document> GetDocumentForDetail(string documentId);
        Task<IEnumerable<Document>> GetFilteredPaginated(DocumentSearchFilters filters);
        Task<IEnumerable<Document>> GetDocumentsToSyncFeap(string anoLetivo);
        Task<int> GetDocumentsToSyncFeapCount(string anoLetivo);
        Task<IEnumerable<Document>> GetPaidDocsToSync(string[] documentIds);
        Task<int> GetPaidDocsToSyncCount(string[] documentIds);
        Task<IEnumerable<Document>> GetPaginatedDocsToSyncSigefe(string anoLetivo, PaginatedSearchFilter filters);
        Task<IEnumerable<Document>> GetDocsToSyncSigefe(string anoLetivo, string documentId = null);
        Task<int> GetDocsToSyncSigefeCount(string anoLetivo);
        Task<IEnumerable<Document>> GetCreditNotesToReprocess(string anoLetivo);
        Task<int> GetCreditNotesToReprocessCount(string anoLetivo);
    }
}
