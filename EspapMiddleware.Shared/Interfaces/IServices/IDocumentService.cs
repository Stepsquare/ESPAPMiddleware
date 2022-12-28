using EspapMiddleware.Shared.DataContracts;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.WebServiceModels;
using EspapMiddleware.Shared.XmlSerializerModel;
using System;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IServices
{
    public interface IDocumentService
    {
        Task AddFailedRequestLog(RequestLogTypeEnum type, Exception ex, Guid uniqueId, string documentId = null);
        Task AddDocument(SendDocumentContract contract);
        Task UpdateDocument(SendDocumentContract contract);
        Task SyncDocument(SetDocumentResultMCIn contract);
    }
}
