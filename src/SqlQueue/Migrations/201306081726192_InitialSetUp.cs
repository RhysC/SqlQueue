namespace SqlQueue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialSetUp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Messages", "ReadDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Messages", "ReadDate");
        }
    }
}
