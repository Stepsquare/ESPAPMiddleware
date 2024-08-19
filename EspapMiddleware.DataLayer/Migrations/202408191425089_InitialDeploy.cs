namespace EspapMiddleware.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDeploy : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Documents", "IsSynchronizedWithSigefe");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Documents", "IsSynchronizedWithSigefe", c => c.Boolean(nullable: false));
        }
    }
}
