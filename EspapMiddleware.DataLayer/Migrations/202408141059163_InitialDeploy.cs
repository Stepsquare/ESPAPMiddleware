namespace EspapMiddleware.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDeploy : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentActions",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.Binary(),
                        ContentType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentLines",
                c => new
                    {
                        DocumentId = c.String(nullable: false, maxLength: 128),
                        LineId = c.Int(nullable: false),
                        Description = c.String(),
                        StandardItemIdentification = c.String(),
                        Quantity = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.DocumentId, t.LineId })
                .ForeignKey("dbo.Documents", t => t.DocumentId, cascadeDelete: true)
                .Index(t => t.DocumentId);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        DocumentId = c.String(nullable: false, maxLength: 128),
                        MEId = c.String(),
                        RelatedDocumentId = c.String(maxLength: 128),
                        ReferenceNumber = c.String(),
                        RelatedReferenceNumber = c.String(),
                        TypeId = c.Int(nullable: false),
                        IssueDate = c.DateTime(nullable: false),
                        SupplierFiscalId = c.String(),
                        CustomerFiscalId = c.String(),
                        InternalManagement = c.String(),
                        SchoolYear = c.String(),
                        CompromiseNumber = c.String(),
                        TotalAmount = c.String(),
                        UblFileId = c.Int(nullable: false),
                        PdfFileId = c.Int(nullable: false),
                        AttachsFileId = c.Int(),
                        StateId = c.Int(nullable: false),
                        StateDate = c.DateTime(nullable: false),
                        ActionId = c.Int(),
                        ActionDate = c.DateTime(),
                        IsMEGA = c.Boolean(nullable: false),
                        IsProcessed = c.Boolean(nullable: false),
                        IsSynchronizedWithSigefe = c.Boolean(nullable: false),
                        IsSynchronizedWithFEAP = c.Boolean(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        UpdatedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.DocumentId)
                .ForeignKey("dbo.DocumentActions", t => t.ActionId)
                .ForeignKey("dbo.DocumentFiles", t => t.AttachsFileId)
                .ForeignKey("dbo.DocumentFiles", t => t.PdfFileId)
                .ForeignKey("dbo.Documents", t => t.RelatedDocumentId)
                .ForeignKey("dbo.DocumentStates", t => t.StateId, cascadeDelete: true)
                .ForeignKey("dbo.DocumentTypes", t => t.TypeId, cascadeDelete: true)
                .ForeignKey("dbo.DocumentFiles", t => t.UblFileId)
                .Index(t => t.RelatedDocumentId)
                .Index(t => t.TypeId)
                .Index(t => t.UblFileId)
                .Index(t => t.PdfFileId)
                .Index(t => t.AttachsFileId)
                .Index(t => t.StateId)
                .Index(t => t.ActionId);
            
            CreateTable(
                "dbo.DocumentMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.String(maxLength: 128),
                        MessageTypeId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        MessageCode = c.String(),
                        MessageContent = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.DocumentMessageTypes", t => t.MessageTypeId, cascadeDelete: true)
                .Index(t => t.DocumentId)
                .Index(t => t.MessageTypeId);
            
            CreateTable(
                "dbo.DocumentMessageTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RequestLogs",
                c => new
                    {
                        UniqueId = c.Guid(nullable: false),
                        RequestLogTypeId = c.Int(nullable: false),
                        DocumentId = c.String(maxLength: 128),
                        SupplierFiscalId = c.String(),
                        ReferenceNumber = c.String(),
                        Date = c.DateTime(nullable: false),
                        Successful = c.Boolean(nullable: false),
                        ExceptionType = c.String(),
                        ExceptionAtFile = c.String(),
                        ExceptionAtLine = c.Int(),
                        ExceptionMessage = c.String(),
                        RequestLogFileId = c.Int(),
                    })
                .PrimaryKey(t => new { t.UniqueId, t.RequestLogTypeId })
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.RequestLogFiles", t => t.RequestLogFileId)
                .ForeignKey("dbo.RequestLogTypes", t => t.RequestLogTypeId, cascadeDelete: true)
                .Index(t => t.RequestLogTypeId)
                .Index(t => t.DocumentId)
                .Index(t => t.RequestLogFileId);
            
            CreateTable(
                "dbo.RequestLogFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.Binary(),
                        ContentType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RequestLogTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentStates",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DocumentLines", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "UblFileId", "dbo.DocumentFiles");
            DropForeignKey("dbo.Documents", "TypeId", "dbo.DocumentTypes");
            DropForeignKey("dbo.Documents", "StateId", "dbo.DocumentStates");
            DropForeignKey("dbo.RequestLogs", "RequestLogTypeId", "dbo.RequestLogTypes");
            DropForeignKey("dbo.RequestLogs", "RequestLogFileId", "dbo.RequestLogFiles");
            DropForeignKey("dbo.RequestLogs", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "RelatedDocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "PdfFileId", "dbo.DocumentFiles");
            DropForeignKey("dbo.DocumentMessages", "MessageTypeId", "dbo.DocumentMessageTypes");
            DropForeignKey("dbo.DocumentMessages", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "AttachsFileId", "dbo.DocumentFiles");
            DropForeignKey("dbo.Documents", "ActionId", "dbo.DocumentActions");
            DropIndex("dbo.RequestLogs", new[] { "RequestLogFileId" });
            DropIndex("dbo.RequestLogs", new[] { "DocumentId" });
            DropIndex("dbo.RequestLogs", new[] { "RequestLogTypeId" });
            DropIndex("dbo.DocumentMessages", new[] { "MessageTypeId" });
            DropIndex("dbo.DocumentMessages", new[] { "DocumentId" });
            DropIndex("dbo.Documents", new[] { "ActionId" });
            DropIndex("dbo.Documents", new[] { "StateId" });
            DropIndex("dbo.Documents", new[] { "AttachsFileId" });
            DropIndex("dbo.Documents", new[] { "PdfFileId" });
            DropIndex("dbo.Documents", new[] { "UblFileId" });
            DropIndex("dbo.Documents", new[] { "TypeId" });
            DropIndex("dbo.Documents", new[] { "RelatedDocumentId" });
            DropIndex("dbo.DocumentLines", new[] { "DocumentId" });
            DropTable("dbo.DocumentTypes");
            DropTable("dbo.DocumentStates");
            DropTable("dbo.RequestLogTypes");
            DropTable("dbo.RequestLogFiles");
            DropTable("dbo.RequestLogs");
            DropTable("dbo.DocumentMessageTypes");
            DropTable("dbo.DocumentMessages");
            DropTable("dbo.Documents");
            DropTable("dbo.DocumentLines");
            DropTable("dbo.DocumentFiles");
            DropTable("dbo.DocumentActions");
        }
    }
}
