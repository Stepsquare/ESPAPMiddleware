using EspapMiddleware.Shared.DataContracts;
using EspapMiddleware.Shared.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EspapMiddleware.Shared.Entities
{
    public class Document
    {
        public string DocumentId { get; set; }
        public string MEId { get; set; }
        public string RelatedDocumentId { get; set; }
        public string ReferenceNumber { get; set; }
        public string RelatedReferenceNumber { get; set; }
        public DocumentTypeEnum TypeId { get; set; }
        public DateTime IssueDate { get; set; }
        public string SupplierFiscalId { get; set; }
        public string CustomerFiscalId { get; set; }
        public string InternalManagement { get; set; }

        public string SchoolYear { get; set; }
        public string CompromiseNumber { get; set; }
        public string TotalAmount { get; set; }

        public byte[] UblFormat { get; set; }
        public byte[] PdfFormat { get; set; }
        public byte[] Attachs { get; set; }

        public DocumentStateEnum StateId { get; set; }
        public DateTime StateDate { get; set; }

        public DocumentActionEnum? ActionId { get; set; }
        public DateTime? ActionDate { get; set; }

        public bool IsSynchronizedWithSigefe { get; set; }
        public bool IsSynchronizedWithFEAP { get; set; }

        public virtual DocumentType Type { get; set; }
        public virtual DocumentState State { get; set; }
        public virtual DocumentAction Action { get; set; }
        public virtual Document RelatedDocument { get; set; }
        public ICollection<RequestLog> RequestLogs { get; set; }
        public ICollection<DocumentLine> DocumentLines { get; set; }
        public ICollection<DocumentMessage> DocumentMessages { get; set; }
    }
}
