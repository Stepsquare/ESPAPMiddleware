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
        Task AddFailedRequestLog(RequestLogTypeEnum type, Exception ex, Guid uniqueId, string supplierFiscalId, string referenceNumber, string documentId);

        [Obsolete("Deprecated version. Use AddDocumentV2.", true)]
        Task AddDocument(SendDocumentContract contract);

        Task AddDocumentV2(SendDocumentContract contract);
        Task UpdateDocument(SendDocumentContract contract);
        Task SyncDocument(SetDocumentResultMCIn contract);
    }
}
