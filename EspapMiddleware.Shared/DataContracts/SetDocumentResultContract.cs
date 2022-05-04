using EspapMiddleware.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.DataContracts
{
    [DataContract(Namespace = "urnl:ElectronicInvoice.B2BClientOperations")]
    public class SetDocumentResultContract
    {
        [DataMember(Order = 1, IsRequired = true)]
        public Guid uniqueId { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public string documentId { get; set; }

        [DataMember(Order = 3, IsRequired = true)]
        public string referenceNumber { get; set; }

        [DataMember(Order = 4, IsRequired = true)]
        public DocumentTypeEnum documentType { get; set; }

        [DataMember(Order = 5, IsRequired = true)]
        public DateTime issueDate { get; set; }

        [DataMember(Order = 6, IsRequired = true)]
        public string supplierFiscalId { get; set; }

        [DataMember(Order = 7, IsRequired = true)]
        public string customerFiscalId { get; set; }

        [DataMember(Order = 8, IsRequired = true)]
        public bool isASuccess { get; set; }

        [DataMember(Order = 9)]
        public Message[] messages { get; set; }

        [DataMember(Order = 10)]
        public DocumentStateEnum? stateId { get; set; }

        [DataMember(Order = 11)]
        public DocumentActionEnum? actionId { get; set; }

        [DataContract(Name= "Message", Namespace = "urnl:ElectronicInvoice.B2BClientOperations")]
        public class Message
        {
            [DataContract]
            public enum MessageType
            {
                [EnumMember(Value = "1")]
                Erro = 1,
                [EnumMember(Value = "2")]
                Aviso = 2,
                [EnumMember(Value = "3")]
                Informação = 3
            }

            [DataMember(Order = 1, IsRequired = true)]
            public MessageType typeId { get; set; }

            [DataMember(Order = 2)]
            public string systemId { get; set; }

            [DataMember(Order = 3)]
            public string code { get; set; }

            [DataMember(Order = 4, IsRequired = true)]
            public string description { get; set; }
        }
    }
}
