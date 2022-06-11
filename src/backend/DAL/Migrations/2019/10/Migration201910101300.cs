using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201910101300)]
    public class AddIsActiveToRole : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Roles", new Column("IsActive", DbType.Boolean, defaultValue: true));
        }
    }
}
