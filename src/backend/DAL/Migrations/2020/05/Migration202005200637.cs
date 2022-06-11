using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202005200637)]
    public class Migration202005200637 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Tariffs", new Column("ExtraPointRate", DbType.Decimal, ColumnProperty.Null));

            Database.AddColumn("ShippingWarehouses", new Column("Latitude", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("ShippingWarehouses", new Column("Longitude", DbType.Decimal, ColumnProperty.Null));

            Database.AddColumn("Warehouses", new Column("Latitude", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("Warehouses", new Column("Longitude", DbType.Decimal, ColumnProperty.Null));

            Database.AddTable("WarehouseDistances",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("ShippingWarehouseId", DbType.Guid, ColumnProperty.NotNull),
                new Column("DeliveryWarehouseId", DbType.Guid, ColumnProperty.NotNull),
                new Column("Distance", DbType.Decimal, ColumnProperty.Null));
        }
    }
}
