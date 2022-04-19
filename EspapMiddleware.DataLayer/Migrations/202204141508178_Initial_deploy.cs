﻿namespace EspapMiddleware.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial_deploy : DbMigration
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
                        RelatedDocumentId = c.String(),
                        ReferenceNumber = c.String(),
                        TypeId = c.Int(nullable: false),
                        IssueDate = c.DateTime(nullable: false),
                        SupplierFiscalId = c.String(),
                        CustomerFiscalId = c.String(),
                        InternalManagement = c.String(),
                        SchoolYear = c.String(),
                        CompromiseNumber = c.String(),
                        TotalAmount = c.String(),
                        UblFormat = c.Binary(),
                        PdfFormat = c.Binary(),
                        Attachs = c.Binary(),
                        StateId = c.Int(nullable: false),
                        StateDate = c.DateTime(nullable: false),
                        ActionId = c.Int(),
                        ActionDate = c.DateTime(),
                        MEId = c.String(),
                        IsSynchronizedWithFEAP = c.Boolean(nullable: false),
                        FEAPMessages = c.String(),
                        RelatedDocument_DocumentId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.DocumentId)
                .ForeignKey("dbo.DocumentActions", t => t.ActionId)
                .ForeignKey("dbo.Documents", t => t.RelatedDocument_DocumentId)
                .ForeignKey("dbo.DocumentStates", t => t.StateId, cascadeDelete: true)
                .ForeignKey("dbo.DocumentTypes", t => t.TypeId, cascadeDelete: true)
                .Index(t => t.TypeId)
                .Index(t => t.StateId)
                .Index(t => t.ActionId)
                .Index(t => t.RelatedDocument_DocumentId);
            
            CreateTable(
                "dbo.RequestLogs",
                c => new
                    {
                        UniqueId = c.Guid(nullable: false),
                        RequestLogTypeId = c.Int(nullable: false),
                        DocumentId = c.String(maxLength: 128),
                        Date = c.DateTime(nullable: false),
                        Successful = c.Boolean(nullable: false),
                        ExceptionType = c.String(),
                        ExceptionStackTrace = c.String(),
                        ExceptionMessage = c.String(),
                    })
                .PrimaryKey(t => t.UniqueId)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.RequestLogTypes", t => t.RequestLogTypeId, cascadeDelete: true)
                .Index(t => t.RequestLogTypeId)
                .Index(t => t.DocumentId);
            
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
            DropForeignKey("dbo.Documents", "TypeId", "dbo.DocumentTypes");
            DropForeignKey("dbo.Documents", "StateId", "dbo.DocumentStates");
            DropForeignKey("dbo.RequestLogs", "RequestLogTypeId", "dbo.RequestLogTypes");
            DropForeignKey("dbo.RequestLogs", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "RelatedDocument_DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "ActionId", "dbo.DocumentActions");
            DropIndex("dbo.RequestLogs", new[] { "DocumentId" });
            DropIndex("dbo.RequestLogs", new[] { "RequestLogTypeId" });
            DropIndex("dbo.Documents", new[] { "RelatedDocument_DocumentId" });
            DropIndex("dbo.Documents", new[] { "ActionId" });
            DropIndex("dbo.Documents", new[] { "StateId" });
            DropIndex("dbo.Documents", new[] { "TypeId" });
            DropIndex("dbo.DocumentLines", new[] { "DocumentId" });
            DropTable("dbo.DocumentTypes");
            DropTable("dbo.DocumentStates");
            DropTable("dbo.RequestLogTypes");
            DropTable("dbo.RequestLogs");
            DropTable("dbo.Documents");
            DropTable("dbo.DocumentLines");
            DropTable("dbo.DocumentActions");
        }
    }
}
