using EspapMiddleware.DataLayer.Context;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Interfaces.IRepositories;
using EspapMiddleware.Shared.MonitorServiceModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        public async Task<DocumentFile> GetDocumentFileForDownload(string documentId, DocumentFileTypeEnum type)
        {
            return await DbContext.DocumentFiles.FirstOrDefaultAsync(x => x.DocumentId == documentId && x.DocumentFileTypeId == type);
        }
    }
}
