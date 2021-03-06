using EspapMiddleware.DataLayer.Context;
using EspapMiddleware.DataLayer.Repositories;
using EspapMiddleware.Shared.Interfaces.IConfiguration;
using EspapMiddleware.Shared.Interfaces.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.DataLayer.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private EspapMiddlewareDbContext dbContext;

        public IDocumentRepository Documents { get; private set; }
        public IRequestLogRepository RequestLogs { get; private set; }

        public UnitOfWork()
        {
            dbContext = new EspapMiddlewareDbContext();
            Documents = new DocumentRepository(dbContext);
            RequestLogs = new RequestLogRepository(dbContext); ;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
