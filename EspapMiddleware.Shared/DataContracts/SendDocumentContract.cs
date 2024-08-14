using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EspapMiddleware.Shared.DataContracts
{
    [DataContract(Namespace = "urn:ElectronicInvoice.B2BClientOperations")]
    public class SendDocumentContract
    {
        private string _ublFormat;

        [DataMember(Order = 1, IsRequired = true)]
        public Guid uniqueId { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public bool isAnUpdate { get; set; }

        [DataMember(Order = 3, IsRequired = true)]
        public string documentId { get; set; }

        [DataMember(Order = 4, IsRequired = true)]
        public string referenceNumber { get; set; }

        [DataMember(Order = 5, IsRequired = true)]
        public DocumentTypeEnum documentType { get; set; }

        [DataMember(Order = 6, IsRequired = true)]
        public DateTime issueDate { get; set; }

        [DataMember(Order = 7, IsRequired = true)]
        public string supplierFiscalId { get; set; }

        [DataMember(Order = 8, IsRequired = true)]
        public string customerFiscalId { get; set; }

        [DataMember(Order = 9)]
        public string internalManagement { get; set; }

        [DataMember(Order = 10)]
        public string feapPortalUser { get; set; }

        [DataMember(Order = 11, IsRequired = true)]
        public string ublFormat
        {
            get { return _ublFormat; }
            set
            {
                try
                {
                    byte[] toDecodeByte = Convert.FromBase64String(value);

                    using (var stream = new MemoryStream(toDecodeByte))
                    {
                        var xmlDoc = new XmlDocument();

                        xmlDoc.Load(stream);

                        ExtractFromUblFormat(xmlDoc);
                    }
                }
                finally
                {
                    _ublFormat = value;
                }
            }
        }

        [DataMember(Order = 12, IsRequired = true)]
        public string pdfFormat { get; set; }

        [DataMember(Order = 13)]
        public string attachs { get; set; }

        [DataMember(Order = 14)]
        public DocumentStateEnum? stateId { get; set; }

        [DataMember(Order = 15)]
        public DateTime? stateDate { get; set; }

        [DataMember(Order = 16)]
        public DocumentActionEnum? actionId { get; set; }

        [DataMember(Order = 17)]
        public DateTime? actionDate { get; set; }

        public string TotalAmount { get; private set; }
        public string CompromiseNumber { get; private set; }
        public string RelatedReferenceNumber { get; private set; }

        public List<InvoiceLineModel> InvoiceLines { get; private set; }

        private void ExtractFromUblFormat(XmlDocument document)
        {
            var nsmgr = new XmlNamespaceManager(document.NameTable);

            nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            InvoiceLines = new List<InvoiceLineModel>();

            switch (this.documentType)
            {
                case DocumentTypeEnum.NotaCrédito:

                    nsmgr.AddNamespace("ubl", "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2");

                    TotalAmount = document.SelectSingleNode("//ubl:CreditNote/cac:LegalMonetaryTotal/cbc:PayableAmount", nsmgr)?.InnerText;

                    CompromiseNumber = document.SelectSingleNode("//ubl:CreditNote/cbc:AccountingCost", nsmgr)?.InnerText;
                    if (string.IsNullOrWhiteSpace(CompromiseNumber))
                        CompromiseNumber = document.SelectSingleNode("//ubl:CreditNote/cac:OrderReference/cbc:ID", nsmgr)?.InnerText;

                    RelatedReferenceNumber = document.SelectSingleNode("//ubl:CreditNote/cac:BillingReference/cac:InvoiceDocumentReference/cbc:ID", nsmgr)?.InnerText;

                    foreach (XmlNode line in document.SelectNodes("//ubl:CreditNote/cac:CreditNoteLine", nsmgr))
                    {
                        if (string.IsNullOrWhiteSpace(CompromiseNumber) && line["cbc:AccountingCost"] != null)
                            CompromiseNumber = line["cbc:AccountingCost"].InnerText;

                        InvoiceLines.Add(new InvoiceLineModel()
                        {
                            Id = line["cbc:ID"]?.InnerText,
                            Name = line["cac:Item"]?["cbc:Name"]?.InnerText,
                            StandardItemIdentification = line["cac:Item"]?["cac:StandardItemIdentification"]?["cbc:ID"]?.InnerText,
                            Quantity = line["cbc:CreditedQuantity"]?.InnerText.Split('.')[0],
                            TotalLineValue = line["cbc:LineExtensionAmount"]?.InnerText,
                            TaxPercent = line["cac:Item"]?["cac:ClassifiedTaxCategory"]?["cbc:Percent"]?.InnerText
                        });
                    }
                    break;
                default:

                    nsmgr.AddNamespace("ubl", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");

                    TotalAmount = document.SelectSingleNode("//ubl:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount", nsmgr)?.InnerText;

                    CompromiseNumber = document.SelectSingleNode("//ubl:Invoice/cbc:AccountingCost", nsmgr)?.InnerText;
                    if (string.IsNullOrWhiteSpace(CompromiseNumber))
                        CompromiseNumber = document.SelectSingleNode("//ubl:Invoice/cac:OrderReference/cbc:ID", nsmgr)?.InnerText;

                    foreach (XmlNode line in document.SelectNodes("//ubl:Invoice/cac:InvoiceLine", nsmgr))
                    {
                        if (string.IsNullOrWhiteSpace(CompromiseNumber))
                        {
                            if (line["cbc:AccountingCost"] != null)
                                CompromiseNumber = line["cbc:AccountingCost"].InnerText;
                            else if (line["cbc:Note"] != null)
                                CompromiseNumber = line["cbc:Note"].InnerText;
                        }

                        InvoiceLines.Add(new InvoiceLineModel()
                        {
                            Id = line["cbc:ID"]?.InnerText,
                            Name = line["cac:Item"]?["cbc:Name"]?.InnerText,
                            StandardItemIdentification = line["cac:Item"]?["cac:StandardItemIdentification"]?["cbc:ID"]?.InnerText,
                            Quantity = line["cbc:InvoicedQuantity"]?.InnerText.Split('.')[0],
                            TotalLineValue = line["cbc:LineExtensionAmount"]?.InnerText,
                            TaxPercent = line["cac:Item"]?["cac:ClassifiedTaxCategory"]?["cbc:Percent"]?.InnerText
                        });
                    }
                    break;
            }
        }

        public class InvoiceLineModel
        {
            public string Id { get; set; }
            public string Quantity { get; set; }
            public string Name { get; set; }
            public string StandardItemIdentification { get; set; }
            public string TotalLineValue { get; set; }
            public string TaxPercent { get; set; }
        }
    }
}
