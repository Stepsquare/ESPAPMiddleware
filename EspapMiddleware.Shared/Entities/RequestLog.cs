using EspapMiddleware.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Entities
{
    public class RequestLog
    {
        public Guid UniqueId { get; set; }
        public RequestLogTypeEnum RequestLogTypeId { get; set; }
        public string DocumentId { get; set; }
        public string SupplierFiscalId { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime Date { get; set; }
        public bool Successful { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionStackTrace { get; set; }
        public string ExceptionMessage { get; set; }

        public virtual RequestLogType RequestLogType { get; set; }
        public virtual Document Document { get; set; }
    }
}
