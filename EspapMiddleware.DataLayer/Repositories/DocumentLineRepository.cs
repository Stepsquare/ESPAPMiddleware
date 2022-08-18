using EspapMiddleware.DataLayer.Context;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Interfaces.IRepositories;
using EspapMiddleware.Shared.MonitorServiceModels;
using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.DataLayer.Repositories
{
    public  class DocumentLineRepository : GenericRepository<DocumentLine>, IDocumentLineRepository
    {
        public DocumentLineRepository(EspapMiddlewareDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DocumentLine>> GetFilteredPaginated(DocumentDetailLineFilter filters)
        {
            return await DbContext.DocumentLines
                                .Where(x => x.DocumentId == filters.DocumentId)
                                .OrderBy(x => x.LineId)
                                .Skip((filters.PageIndex - 1) * filters.PageSize).Take(filters.PageSize)
                                .ToListAsync();
        }
    }
}
