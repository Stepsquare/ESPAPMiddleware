using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.MonitorServiceModels
{
    public class RequestLogSearchFilters : PaginatedSearchFilter
    {
        public Guid? UniqueId { get; set; }
        public RequestLogTypeEnum? Type { get; set; }
        public string SupplierFiscalId { get; set; }
        public string ReferenceNumber { get; set; }
        public bool? IsSuccessFul { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? UntilDate { get; set; }
    }
}
