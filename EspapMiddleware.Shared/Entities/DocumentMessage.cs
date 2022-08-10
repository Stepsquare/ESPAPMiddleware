using EspapMiddleware.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Entities
{
    public class DocumentMessage
    {
        public int Id { get; set; }
        public string DocumentId { get; set; }
        public DocumentMessageTypeEnum MessageTypeId { get; set; }
        public DateTime Date { get; set; }
        public string MessageCode { get; set; }
        public string MessageContent { get; set; }

        public virtual Document Document { get; set; }
        public virtual DocumentMessageType MessageType { get; set; }
    }
}
