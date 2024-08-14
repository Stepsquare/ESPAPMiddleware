using EspapMiddleware.Shared.ConfigModels;
using EspapMiddleware.Shared.DataContracts;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Exceptions;
using EspapMiddleware.Shared.Interfaces.IServices;
using EspapMiddleware.Shared.WebServiceModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Xml;

namespace EspapMiddleware.WcfService
{
    public class Service : IService
    {
        private readonly IWcfServices _service;

        public Service(IWcfServices service)
        {
            _service = service;
        }

        public void SendDocument(SendDocumentRequest request)
        {
            DateTime receivedOn = DateTime.Now;

            try
            {
                if (request.SendDocumentMCIn.isAnUpdate)
                {
                    _service.UpdateDocument(request.SendDocumentMCIn).GetAwaiter().GetResult();
                }
                else
                {
                    var insertedDocument = _service.AddDocument(request.SendDocumentMCIn).GetAwaiter().GetResult();

                    if (insertedDocument.TypeId == DocumentTypeEnum.Fatura)
                        _service.ProcessInvoice(insertedDocument).GetAwaiter().GetResult();

                    if (insertedDocument.TypeId == DocumentTypeEnum.NotaCrédito)
                        _service.ProcessCreditNote(insertedDocument).GetAwaiter().GetResult();
                }

                _service.AddSuccessRequestLog(
                    RequestLogTypeEnum.SendDocument,
                    request.SendDocumentMCIn.uniqueId,
                    request.SendDocumentMCIn.supplierFiscalId,
                    request.SendDocumentMCIn.referenceNumber,
                    request.SendDocumentMCIn.documentId,
                    receivedOn
                ).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _service.AddFailedRequestLog(
                    ex,
                    RequestLogTypeEnum.SendDocument,
                    request.SendDocumentMCIn.uniqueId,
                    request.SendDocumentMCIn.supplierFiscalId,
                    request.SendDocumentMCIn.referenceNumber,
                    request.SendDocumentMCIn.documentId,
                    receivedOn
                ).GetAwaiter().GetResult();

                throw ex;
            }
        }

        public void SetDocumentResult(SetDocumentResultRequest request)
        {
            DateTime receivedOn = DateTime.Now;

            try
            {
                _service.SyncDocument(request.SetDocumentResultMCIn).GetAwaiter().GetResult();

                _service.AddSuccessRequestLog(
                    RequestLogTypeEnum.SendDocument,
                    request.SetDocumentResultMCIn.uniqueId,
                    request.SetDocumentResultMCIn.supplierFiscalId,
                    request.SetDocumentResultMCIn.referenceNumber,
                    request.SetDocumentResultMCIn.documentId,
                    receivedOn
                ).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _service.AddFailedRequestLog(
                    ex,
                    RequestLogTypeEnum.SetDocumentResult,
                    request.SetDocumentResultMCIn.uniqueId,
                    request.SetDocumentResultMCIn.supplierFiscalId,
                    request.SetDocumentResultMCIn.referenceNumber,
                    request.SetDocumentResultMCIn.documentId,
                    receivedOn
                ).GetAwaiter().GetResult();

                throw ex;
            }
        }
    }
}
