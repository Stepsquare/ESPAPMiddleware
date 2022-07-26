namespace EspapMiddleware.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_DocumentMessages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.String(maxLength: 128),
                        MessageTypeId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
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
            
            AddColumn("dbo.Documents", "IsSynchronizedWithSigefe", c => c.Boolean(nullable: false));
            DropColumn("dbo.Documents", "FEAPMessages");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Documents", "FEAPMessages", c => c.String());
            DropForeignKey("dbo.DocumentMessages", "MessageTypeId", "dbo.DocumentMessageTypes");
            DropForeignKey("dbo.DocumentMessages", "DocumentId", "dbo.Documents");
            DropIndex("dbo.DocumentMessages", new[] { "MessageTypeId" });
            DropIndex("dbo.DocumentMessages", new[] { "DocumentId" });
            DropColumn("dbo.Documents", "IsSynchronizedWithSigefe");
            DropTable("dbo.DocumentMessageTypes");
            DropTable("dbo.DocumentMessages");
        }
    }
}
