using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.MonitorServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IRepositories
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        Task<Document> GetByIdIncludeRelatedDoc(string DocumentId);
        Task<IEnumerable<Document>> GetFilteredPaginated(DocumentSearchFilters filters);
        Task<IEnumerable<string>> GetSchoolYears();
    }
}
