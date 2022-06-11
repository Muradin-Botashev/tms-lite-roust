using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202001201840)]
    public class AlterColumnsToOrders : Migration
    {
        public override void Apply()
        {
            Database.RemoveColumn("Orders", "DocumentAttached");
            Database.AddColumn("Orders", new Column("DocumentAttached", DbType.Boolean, ColumnProperty.NotNull, defaultValue: false));
        }
    }
}
