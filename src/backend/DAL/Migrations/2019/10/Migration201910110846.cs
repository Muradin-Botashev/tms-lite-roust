using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201910110846)]
    public class AddOrderIsActive : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("IsActive", DbType.Boolean, defaultValue: true));
        }
    }
}
