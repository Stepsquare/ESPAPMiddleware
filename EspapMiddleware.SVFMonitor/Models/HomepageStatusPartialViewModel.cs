using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EspapMiddleware.SVFMonitor.Models
{
    public class HomepageStatusPartialViewModel
    {
        public int Total { get; set; }
        public int TotalNotSyncFeap { get; set; }
        public bool IsCurrentSchoolYear { get; set; }

        public InvoiceStatusObject InvoiceStatus { get; set; }
        public CreditNoteStatusObject CreditNoteStatus { get; set; }
        public DebitNoteStatusObject DebitNoteStatus { get; set; }

        public class InvoiceStatusObject
        {
            public int Total { get; set; }
            public int PendingRegularization { get; set; }
            public int Regularized { get; set; }
            public int Validated { get; set; }
            public int ValidatedToSync { get; set; }
            public int Paid { get; set; }
        }

        public class CreditNoteStatusObject
        {
            public int Total { get; set; }
            public int Unprocessed { get; set; }
            public int Processed { get; set; }
            public int Returned { get; set; }
        }

        public class DebitNoteStatusObject
        {
            public int Total { get; set; }
            public int Unprocessed { get; set; }
            public int Returned { get; set; }
        }
    }
}