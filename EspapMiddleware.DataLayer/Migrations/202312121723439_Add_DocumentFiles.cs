namespace EspapMiddleware.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_DocumentFiles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentFiles",
                c => new
                    {
                        DocumentId = c.String(nullable: false, maxLength: 128),
                        DocumentFileTypeId = c.Int(nullable: false),
                        Content = c.Binary(),
                        ContentType = c.String(),
                    })
                .PrimaryKey(t => new { t.DocumentId, t.DocumentFileTypeId })
                .ForeignKey("dbo.Documents", t => t.DocumentId, cascadeDelete: true)
                .ForeignKey("dbo.DocumentFileTypes", t => t.DocumentFileTypeId, cascadeDelete: true)
                .Index(t => t.DocumentId)
                .Index(t => t.DocumentFileTypeId);
            
            CreateTable(
                "dbo.DocumentFileTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.Documents", "UblFormat");
            DropColumn("dbo.Documents", "PdfFormat");
            DropColumn("dbo.Documents", "Attachs");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Documents", "Attachs", c => c.Binary());
            AddColumn("dbo.Documents", "PdfFormat", c => c.Binary());
            AddColumn("dbo.Documents", "UblFormat", c => c.Binary());
            DropForeignKey("dbo.DocumentFiles", "DocumentFileTypeId", "dbo.DocumentFileTypes");
            DropForeignKey("dbo.DocumentFiles", "DocumentId", "dbo.Documents");
            DropIndex("dbo.DocumentFiles", new[] { "DocumentFileTypeId" });
            DropIndex("dbo.DocumentFiles", new[] { "DocumentId" });
            DropTable("dbo.DocumentFileTypes");
            DropTable("dbo.DocumentFiles");
        }
    }
}
