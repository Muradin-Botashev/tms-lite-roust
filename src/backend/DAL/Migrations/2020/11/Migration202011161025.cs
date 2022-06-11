using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202011161025)]
    public class Migration202011161025 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("FixedDirections",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("CarrierId", DbType.Guid, ColumnProperty.Null),
                new Column("ShippingWarehouseId", DbType.Guid, ColumnProperty.Null),
                new Column("DeliveryWarehouseId", DbType.Guid, ColumnProperty.Null),
                new Column("ShippingCity", DbType.String, ColumnProperty.Null),
                new Column("DeliveryCity", DbType.String, ColumnProperty.Null),
                new Column("ShippingRegion", DbType.String, ColumnProperty.Null),
                new Column("DeliveryRegion", DbType.String, ColumnProperty.Null),
                new Column("VehicleTypeId", DbType.Guid, ColumnProperty.Null),
                new Column("Quota", DbType.Decimal, ColumnProperty.Null),
                new Column("IsActive", DbType.Boolean, ColumnProperty.NotNull, defaultValue: true));
        }
    }
}
