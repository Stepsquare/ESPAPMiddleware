using EspapMiddleware.DataLayer.Context;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Interfaces.IRepositories;
using EspapMiddleware.Shared.MonitorServiceModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.DataLayer.Repositories
{
    public class RequestLogRepository : GenericRepository<RequestLog>, IRequestLogRepository
    {
        public RequestLogRepository(EspapMiddlewareDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RequestLog>> GetFilteredPaginated(RequestLogSearchFilters filters)
        {
            return await DbContext.RequestLogs
                                .Where(x => (!filters.UniqueId.HasValue || x.UniqueId == filters.UniqueId)
                                    && (!filters.Type.HasValue || x.RequestLogTypeId == filters.Type)
                                    && (!filters.IsSuccessFul.HasValue || x.Successful == filters.IsSuccessFul)
                                    && (!filters.FromDate.HasValue || x.Date > filters.FromDate)
                                    && (!filters.UntilDate.HasValue || x.Date < filters.UntilDate))
                                .OrderByDescending(x => x.Date)
                                .Skip((filters.PageIndex - 1) * filters.PageSize).Take(filters.PageSize)
                                .ToListAsync();
        }
    }
}
