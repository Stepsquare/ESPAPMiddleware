using EspapMiddleware.DataLayer.Context;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Interfaces.IRepositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.DataLayer.Repositories
{
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        protected readonly DbSet<Document> DbSet;

        public DocumentRepository(EspapMiddlewareDbContext context) : base(context)
        {
            DbSet = context.Set<Document>();
        }

        public async Task<Document> GetByIdIncludeRelatedDoc(string documentId)
        {
            return await DbSet
                .Include(x => x.DocumentLines)
                .Include(x => x.RelatedDocument)
                .FirstOrDefaultAsync(x => x.DocumentId == documentId);
        }
    }
}
