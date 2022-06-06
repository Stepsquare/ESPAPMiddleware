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
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<DocumentState> DocumentStates { get; set; }
        public DbSet<DocumentAction> DocumentActions { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }
        public DbSet<RequestLogType> RequestLogTypes { get; set; }

        public EspapMiddlewareDbContext() : base("name=EspapMiddlewareConnectionString")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RequestLog>().HasKey(x => new { x.UniqueId, x.RequestLogTypeId });
            modelBuilder.Entity<RequestLog>().HasOptional(x => x.Document).WithMany(x => x.RequestLogs).HasForeignKey(x => x.DocumentId);

            modelBuilder.Entity<DocumentLine>().HasKey(x => new { x.DocumentId, x.LineId });
            modelBuilder.Entity<DocumentLine>().HasRequired(x => x.Document).WithMany(x => x.DocumentLines).HasForeignKey(x => x.DocumentId);
        }
    }
}
