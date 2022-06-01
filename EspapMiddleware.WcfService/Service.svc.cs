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
        private readonly IDocumentService _service;

        public Service(IDocumentService service)
        {
            _service = service;
        }

        public void SendDocument(SendDocumentRequest request)
        {
            try
            {
#if DEBUG
                request.SendDocumentMCIn.Validate();
#endif

                if (request.SendDocumentMCIn.isAnUpdate)
                {
                    _service.UpdateDocument(request.SendDocumentMCIn).GetAwaiter().GetResult();
                }
                else
                {
                    _service.AddDocument(request.SendDocumentMCIn).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _service.AddFailedRequestLog(RequestLogTypeEnum.SendDocument, ex, request.SendDocumentMCIn.uniqueId, request.SendDocumentMCIn.documentId);

                throw ex;
            }
        }

        public void SetDocumentResult(SetDocumentResultRequest request)
        {
            try
            {
                _service.SyncDocument(request.SetDocumentResultMCIn).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _service.AddFailedRequestLog(RequestLogTypeEnum.SetDocumentResult, ex, request.SetDocumentResultMCIn.uniqueId, request.SetDocumentResultMCIn.documentId);

                throw ex;
            }
        }
    }
}
