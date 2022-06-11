using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201911251344)]
    public class Migration201911251344 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("CarrierId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("DeliveryType", DbType.Int32, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("DeviationsComment", DbType.String.WithSize(1000), ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("DeliveryCost", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("ActualDeliveryCost", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null));

            Database.ExecuteNonQuery($@"
                update ""Orders""
                set ""CarrierId"" = (select ""Shippings"".""CarrierId"" from ""Shippings"" where ""Shippings"".""Id"" = ""Orders"".""ShippingId"" limit 1)
                where ""ShippingId"" is not null;

                update ""Orders""
                set ""DeliveryType"" = (select ""Warehouses"".""DeliveryType"" from ""Warehouses"" where ""Warehouses"".""Id"" = ""Orders"".""DeliveryWarehouseId"" limit 1)
                where ""DeliveryWarehouseId"" is not null;
            ");
        }
    }
}
