using EspapMiddleware.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Entities
{
    public class RequestLogType
    {
        public RequestLogTypeEnum Id { get; set; }
        public string Description { get; set; }
    }
}
