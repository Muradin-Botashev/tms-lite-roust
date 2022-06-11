using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911131610)]
    public class AddOrdersColumns : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("DocumentReturnStatus", DbType.Boolean, defaultValue: false));
        }
    }
}
