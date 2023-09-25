namespace EspapMiddleware.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RequestLog_Add_SupplierFiscalId_ReferenceNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RequestLogs", "SupplierFiscalId", c => c.String());
            AddColumn("dbo.RequestLogs", "ReferenceNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestLogs", "ReferenceNumber");
            DropColumn("dbo.RequestLogs", "SupplierFiscalId");
        }
    }
}
