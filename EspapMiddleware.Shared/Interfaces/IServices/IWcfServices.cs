using EspapMiddleware.Shared.DataContracts;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.WebServiceModels;
using EspapMiddleware.Shared.XmlSerializerModel;
using System;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IServices
{
    public interface IWcfServices
    {
        Task AddSuccessRequestLog(RequestLogTypeEnum type, Guid uniqueId, string supplierFiscalId, string referenceNumber, string documentId, DateTime receivedOn);
        Task AddFailedRequestLog(Exception ex, RequestLogTypeEnum type, Guid uniqueId, string supplierFiscalId, string referenceNumber, string documentId, DateTime receivedOn);
        
        Task<Document> AddDocument(SendDocumentContract contract);
        Task ProcessInvoice(Document document);
        Task ProcessCreditNote(Document document);

        Task UpdateDocument(SendDocumentContract contract);
        Task SyncDocument(SetDocumentResultMCIn contract);
    }
}
