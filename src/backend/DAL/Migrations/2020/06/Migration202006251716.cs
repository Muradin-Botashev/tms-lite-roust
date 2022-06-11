using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202006251716)]
    public class Migration202006251716 : Migration
    {
        public override void Apply()
        {
            Database.RemoveTable("AutogroupingOrders");

            Database.AddTable("AutogroupingOrders",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("RunId", DbType.Guid, ColumnProperty.NotNull),
                new Column("AutogroupingShippingId", DbType.Guid, ColumnProperty.Null),
                new Column("OrderId", DbType.Guid, ColumnProperty.NotNull),
                new Column("OrderNumber", DbType.String, ColumnProperty.Null),
                new Column("ShippingWarehouseId", DbType.Guid, ColumnProperty.Null),
                new Column("DeliveryWarehouseId", DbType.Guid, ColumnProperty.Null),
                new Column("DeliveryRegion", DbType.String.WithSize(255), ColumnProperty.Null),
                new Column("ShippingDate", DbType.DateTime, ColumnProperty.Null),
                new Column("DeliveryDate", DbType.DateTime, ColumnProperty.Null),
                new Column("DeliveryTime", DbType.Time, ColumnProperty.Null),
                new Column("PalletsCount", DbType.Int32, ColumnProperty.Null),
                new Column("WeightKg", DbType.Decimal, ColumnProperty.Null),
                new Column("VehicleTypeId", DbType.Guid, ColumnProperty.Null),
                new Column("BodyTypeId", DbType.Guid, ColumnProperty.Null),
                new Column("CreatedAt", DbType.DateTime, ColumnProperty.NotNull));

            Database.AddTable("AutogroupingShippings",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("RunId", DbType.Guid, ColumnProperty.NotNull),
                new Column("ShippingNumber", DbType.String, ColumnProperty.Null),
                new Column("RouteNumber", DbType.Int32, ColumnProperty.Null),
                new Column("ShippingDate", DbType.DateTime, ColumnProperty.Null),
                new Column("DeliveryDate", DbType.DateTime, ColumnProperty.Null),
                new Column("PalletsCount", DbType.Int32, ColumnProperty.Null),
                new Column("WeightKg", DbType.Decimal, ColumnProperty.Null),
                new Column("Route", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null),
                new Column("CarrierId", DbType.Guid, ColumnProperty.Null),
                new Column("TarifficationType", DbType.Int32, ColumnProperty.Null),
                new Column("AutogroupingType", DbType.Int32, ColumnProperty.Null),
                new Column("BestCost", DbType.Decimal, ColumnProperty.Null),
                new Column("FtlDirectCost", DbType.Decimal, ColumnProperty.Null),
                new Column("FtlRouteCost", DbType.Decimal, ColumnProperty.Null),
                new Column("LtlCost", DbType.Decimal, ColumnProperty.Null),
                new Column("PoolingCost", DbType.Decimal, ColumnProperty.Null),
                new Column("MilkrunCost", DbType.Decimal, ColumnProperty.Null),
                new Column("CreatedAt", DbType.DateTime, ColumnProperty.NotNull),
                new Column("FtlDirectCostMessage", DbType.String.WithSize(1000), ColumnProperty.Null),
                new Column("FtlRouteCostMessage", DbType.String.WithSize(1000), ColumnProperty.Null),
                new Column("LtlCostMessage", DbType.String.WithSize(1000), ColumnProperty.Null),
                new Column("MilkrunCostMessage", DbType.String.WithSize(1000), ColumnProperty.Null),
                new Column("PoolingCostMessage", DbType.String.WithSize(1000), ColumnProperty.Null));
        }
    }
}
