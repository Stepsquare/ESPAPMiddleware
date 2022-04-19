using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EspapMiddleware.Shared.DataContracts.SendDocumentContract;

namespace EspapMiddleware.Shared.Entities
{
    public class DocumentLine
    {
        public int LineId { get; set; }
        public string DocumentId { get; set; }
        public string Description  { get; set; }
        public string StandardItemIdentification { get; set; }
        public int Quantity  { get; set; }
        public decimal Value  { get; set; }
        public decimal TaxPercentage  { get; set; }

        public virtual Document Document { get; set; }
    }
}
