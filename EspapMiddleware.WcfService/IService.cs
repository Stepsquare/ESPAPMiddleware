﻿using EspapMiddleware.Shared.DataContracts;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.WebServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace EspapMiddleware.WcfService
{
    [ServiceContract]
    [ServiceKnownType(typeof(DocumentTypeEnum))]
    [ServiceKnownType(typeof(DocumentStateEnum))]
    [ServiceKnownType(typeof(DocumentActionEnum))]
    [ServiceKnownType(typeof(SetDocumentResultContract.Message.MessageType))]
    public interface IService
    {
        [OperationContract(IsOneWay = true)]
        void SendDocument(SendDocumentRequest request);

        [OperationContract(IsOneWay = true)]
        void SetDocumentResult(SetDocumentResultRequest request);
    }

    [MessageContract(IsWrapped = false)]
    public class SendDocumentRequest
    {
        [MessageBodyMember(Namespace = "urnl:ElectronicInvoice.B2BClientOperations")]
        public SendDocumentContract SendDocumentMCIn { get; set; }
    }

    [MessageContract(IsWrapped = false)]
    public class SetDocumentResultRequest
    {
        [MessageBodyMember(Namespace = "urnl:ElectronicInvoice.B2BClientOperations")]
        public SetDocumentResultContract SetDocumentResultMCIn { get; set; }
    }
}
