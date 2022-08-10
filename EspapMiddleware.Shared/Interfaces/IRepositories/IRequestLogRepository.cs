using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.MonitorServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IRepositories
{
    public interface IRequestLogRepository: IGenericRepository<RequestLog>
    {
        Task<IEnumerable<RequestLog>> GetFilteredPaginated(RequestLogSearchFilters filters);
    }
}
