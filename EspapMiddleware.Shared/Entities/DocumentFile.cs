using EspapMiddleware.Shared.Enums;

namespace EspapMiddleware.Shared.Entities
{
    public  class DocumentFile
    {
        public string DocumentId { get; set; }
        public DocumentFileTypeEnum DocumentFileTypeId { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }

        public virtual DocumentFileType DocumentFileType { get; set; }
        public virtual Document Document { get; set; }
    }
}
