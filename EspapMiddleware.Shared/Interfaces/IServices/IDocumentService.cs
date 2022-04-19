using EspapMiddleware.Shared.DataContracts;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.WebServiceModels;
using System;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IServices
{
    public interface IDocumentService
    {
        Task<GetDocFaturacaoResponse> GetDocFaturacao(string nif = null, string id_doc_feap = null);
        Task AddFailedRequestLog(RequestLogTypeEnum type, Exception ex, Guid uniqueId, string documentId = null);
        Task AddDocument(SendDocumentContract contract);
        Task UpdateDocument(SendDocumentContract contract);
        Task SyncDocument(SetDocumentResultContract contract);
    }
}
