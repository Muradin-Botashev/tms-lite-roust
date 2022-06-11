using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202011201441)]
    public class Migration202011201441 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("TransportZone", DbType.String, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("BottlesCount", DbType.Int32, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("Volume9l", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("PaymentCondition", DbType.String, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("RouteNumber", DbType.String, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("BottlesCount", DbType.Int32, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("Volume9l", DbType.Decimal, ColumnProperty.Null));
        }
    }
}