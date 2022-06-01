using EspapMiddleware.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.XmlSerializerModel
{
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "urn:ElectronicInvoice.B2BClientOperations")]
    [System.Xml.Serialization.XmlRoot(Namespace = "urn:ElectronicInvoice.B2BClientOperations", IsNullable = false)]
    public partial class SetDocumentResultMCIn
    {

        private Guid uniqueIdField;

        private string documentIdField;

        private string referenceNumberField;

        private int? documentTypeField;

        private DateTime? issueDateField;

        private string supplierFiscalIdField;

        private string customerFiscalIdField;

        private bool isASuccessField;

        private SetDocumentResultMCInMessages[] messagesField;

        private int? stateIdField;

        private int? actionIdField;

        /// <remarks/>
        public Guid uniqueId
        {
            get
            {
                return uniqueIdField;
            }
            set
            {
                uniqueIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public string documentId
        {
            get
            {
                return documentIdField;
            }
            set
            {
                documentIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public string referenceNumber
        {
            get
            {
                return referenceNumberField;
            }
            set
            {
                referenceNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public int? documentType
        {
            get
            {
                return documentTypeField;
            }
            set
            {
                documentTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public DateTime? issueDate
        {
            get
            {
                return issueDateField;
            }
            set
            {
                issueDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public string supplierFiscalId
        {
            get
            {
                return supplierFiscalIdField;
            }
            set
            {
                supplierFiscalIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public string customerFiscalId
        {
            get
            {
                return customerFiscalIdField;
            }
            set
            {
                customerFiscalIdField = value;
            }
        }

        /// <remarks/>
        public bool isASuccess
        {
            get
            {
                return isASuccessField;
            }
            set
            {
                isASuccessField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("messages", IsNullable = true)]
        public SetDocumentResultMCInMessages[] messages
        {
            get
            {
                return messagesField;
            }
            set
            {
                messagesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public int? stateId
        {
            get
            {
                return stateIdField;
            }
            set
            {
                stateIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public int? actionId
        {
            get
            {
                return actionIdField;
            }
            set
            {
                actionIdField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "urn:ElectronicInvoice.B2BClientOperations")]
    public partial class SetDocumentResultMCInMessages
    {

        private int typeIdField;

        private string systemIdField;

        private string codeField;

        private string descriptionField;

        /// <remarks/>
        public int typeId
        {
            get
            {
                return typeIdField;
            }
            set
            {
                typeIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public string systemId
        {
            get
            {
                return systemIdField;
            }
            set
            {
                systemIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public string code
        {
            get
            {
                return codeField;
            }
            set
            {
                codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public string description
        {
            get
            {
                return descriptionField;
            }
            set
            {
                descriptionField = value;
            }
        }
    }
}
