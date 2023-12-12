using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IRepositories
{
    public interface IDocumentFileRepository : IGenericRepository<DocumentFile>
    {
        Task<DocumentFile> GetDocumentFileForDownload(string documentId, DocumentFileTypeEnum type);
    }
}
