﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EspapMiddleware.SVFMonitor.Models
{
    public class HomepageStatusPartialViewModel
    {
        public string SchoolYear { get; set; }
        public int TotalDocuments { get; set; }
        public int TotalValidDocuments { get; set; }
        public int TotalInvalidDocuments { get; set; }
        public int TotalInvalidDocumentsRectified { get; set; }
        public int TotalPaidDocuments { get; set; }
    }
}