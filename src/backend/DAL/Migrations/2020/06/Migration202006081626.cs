using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202006081626)]
    public class Migration202006081626 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("AutogroupingOrders", new Column("RouteNumber", DbType.Int32, ColumnProperty.Null));
        }
    }
}
