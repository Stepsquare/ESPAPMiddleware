using EspapMiddleware.DataLayer.Context;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Interfaces.IRepositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.DataLayer.Repositories
{
    public class DocumenMessageRepository : GenericRepository<DocumentMessage>, IDocumentMessageRepository
    {
        public DocumenMessageRepository(EspapMiddlewareDbContext context) : base(context)
        {
        }

        public async Task<DocumentMessage> GetLastSigefeMessage(string documentId)
        {
            return await DbContext.DocumentMessages
                        .Where(x => x.DocumentId == documentId && x.MessageTypeId == DocumentMessageTypeEnum.SIGeFE)
                        .OrderByDescending(x => x.Date)
                        .FirstOrDefaultAsync();
        }
    }
}
