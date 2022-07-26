namespace EspapMiddleware.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_RelatedReferenceNumber_Documents : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Documents", "RelatedReferenceNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Documents", "RelatedReferenceNumber");
        }
    }
}
