using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202003041233)]
    public class Migration202003041233 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("IsReturn", DbType.Boolean, ColumnProperty.Null));
        }
    }
}
