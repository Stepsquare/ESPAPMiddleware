using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.MonitorServiceModels
{
    public class DocumentSearchFilters : PaginatedSearchFilter
    {
        public string DocumentId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? UntilDate { get; set; }
        public string SupplierFiscalId { get; set; }
        public string SchoolYear { get; set; }
        public string CompromiseNumber { get; set; }
        public DocumentTypeEnum? Type { get; set; }
        public DocumentStateEnum? State { get; set; }
        public string MeId { get; set; }
        public bool? FeapSyncronized { get; set; }
        public bool? SigefeSyncronized { get; set; }
    }
}
