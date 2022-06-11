using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202001141235)]
    public class AddNewColumnsToOrders : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("DowntimeAmount", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("OtherExpenses", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("TotalAmount", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("ManualTotalAmount", DbType.Boolean, defaultValue: false));
            Database.AddColumn("Orders", new Column("TotalAmountNds", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("ManualTotalAmountNds", DbType.Boolean, defaultValue: false));
            Database.AddColumn("Orders", new Column("ReturnShippingCost", DbType.Decimal, ColumnProperty.Null));
        }
    }
}
