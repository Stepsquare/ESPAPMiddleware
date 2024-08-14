using EspapMiddleware.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.DataLayer.Context
{
    public class EspapMiddlewareDbContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentLine> DocumentLines { get; set; }
        public DbSet<DocumentFile> DocumentFiles { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<DocumentState> DocumentStates { get; set; }
        public DbSet<DocumentAction> DocumentActions { get; set; }
        public DbSet<DocumentMessage> DocumentMessages { get; set; }
        public DbSet<DocumentMessageType> DocumentMessagesTypes { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }
        public DbSet<RequestLogType> RequestLogTypes { get; set; }
        public DbSet<RequestLogFile> RequestLogFiles { get; set; }

        public EspapMiddlewareDbContext() : base("name=EspapMiddlewareConnectionString")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>().Property(x => x.CreatedOn).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);

            modelBuilder.Entity<Document>().HasOptional(x => x.RelatedDocument).WithMany(x => x.RelatedDocuments).HasForeignKey(x => x.RelatedDocumentId);
            modelBuilder.Entity<Document>().HasRequired(x => x.UblFile).WithMany().HasForeignKey(x => x.UblFileId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Document>().HasRequired(x => x.PdfFile).WithMany().HasForeignKey(x => x.PdfFileId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Document>().HasOptional(x => x.AttachsFile).WithMany().HasForeignKey(x => x.AttachsFileId);

            modelBuilder.Entity<DocumentLine>().HasKey(x => new { x.DocumentId, x.LineId });
            modelBuilder.Entity<DocumentLine>().HasRequired(x => x.Document).WithMany(x => x.DocumentLines).HasForeignKey(x => x.DocumentId);

            modelBuilder.Entity<RequestLog>().HasKey(x => new { x.UniqueId, x.RequestLogTypeId });
            modelBuilder.Entity<RequestLog>().HasOptional(x => x.Document).WithMany(x => x.RequestLogs).HasForeignKey(x => x.DocumentId);
            modelBuilder.Entity<RequestLog>().HasOptional(x => x.RequestLogFile).WithMany().HasForeignKey(x => x.RequestLogFileId);
        }
    }
}
