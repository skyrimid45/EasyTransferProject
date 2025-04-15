namespace Project.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Account",
                c => new
                    {
                        AccountID = c.Int(nullable: false, identity: true),
                        accountnum = c.Int(nullable: false),
                        CustId = c.Int(),
                    })
                .PrimaryKey(t => t.AccountID)
                .ForeignKey("dbo.Customers", t => t.CustId)
                .Index(t => t.CustId);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        CustId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        Email = c.String(maxLength: 100),
                        Telephone = c.String(),
                    })
                .PrimaryKey(t => t.CustId);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustId = c.Int(),
                        AccountID = c.Int(),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Account", t => t.AccountID)
                .ForeignKey("dbo.Customers", t => t.CustId)
                .Index(t => t.CustId)
                .Index(t => t.AccountID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "CustId", "dbo.Customers");
            DropForeignKey("dbo.Transactions", "AccountID", "dbo.Account");
            DropForeignKey("dbo.Account", "CustId", "dbo.Customers");
            DropIndex("dbo.Transactions", new[] { "AccountID" });
            DropIndex("dbo.Transactions", new[] { "CustId" });
            DropIndex("dbo.Account", new[] { "CustId" });
            DropTable("dbo.Transactions");
            DropTable("dbo.Customers");
            DropTable("dbo.Account");
        }
    }
}
