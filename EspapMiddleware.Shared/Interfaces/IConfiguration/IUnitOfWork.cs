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
        IRequestLogRepository RequestLogs { get; }
        IDocumentMessageRepository DocumentMessages { get; }
        Task<int> SaveChangesAsync();
    }
}
