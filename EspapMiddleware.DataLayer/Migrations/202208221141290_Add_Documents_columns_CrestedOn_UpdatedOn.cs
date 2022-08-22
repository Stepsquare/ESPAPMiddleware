namespace EspapMiddleware.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Documents_columns_CrestedOn_UpdatedOn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Documents", "CreatedOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.Documents", "UpdatedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Documents", "UpdatedOn");
            DropColumn("dbo.Documents", "CreatedOn");
        }
    }
}
