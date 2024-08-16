using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EspapMiddleware.SVFMonitor.Models
{
    public class HomepageViewModel
    {
        public Dictionary<string, string> SchoolYears { get; set; }
        public string CurrentSchoolYear { get; set; }
    }
}