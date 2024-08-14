using EspapMiddleware.Shared.Interfaces.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IConfiguration
{
    public interface IUnitOfWork : IDisposable
    {
        IDocumentRepository Documents { get; }
        IDocumentMessageRepository DocumentMessages { get; }
        IDocumentLineRepository DocumentLines { get; }
        IDocumentFileRepository DocumentFiles { get; }
        IRequestLogRepository RequestLogs { get; }
        IRequestLogFileRepository RequestLogFiles { get; }
        Task<int> SaveChangesAsync();
    }
}
