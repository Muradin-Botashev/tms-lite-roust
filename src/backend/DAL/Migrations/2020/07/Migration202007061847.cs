using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202007061847)]
    public class Migration202007061847 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("AutogroupingShippings", new Column("OrdersCount", DbType.Int32, ColumnProperty.Null));
        }
    }
}
