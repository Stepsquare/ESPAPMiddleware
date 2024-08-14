using EspapMiddleware.DataLayer.Context;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Interfaces.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.DataLayer.Repositories
{
    public class DocumentFileRepository : GenericRepository<DocumentFile>, IDocumentFileRepository
    {
        public DocumentFileRepository(EspapMiddlewareDbContext context) : base(context)
        {
        }
    }
}
