using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.MonitorServiceModels
{
    public class DocumentDetailLineFilter : PaginatedSearchFilter
    {
        public string DocumentId { get; set; }
    }
}
