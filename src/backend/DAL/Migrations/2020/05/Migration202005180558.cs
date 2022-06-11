using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202005180558)]
    public class Migration202005180558 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("AutogroupingOrders",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("RunId", DbType.Guid, ColumnProperty.NotNull),
                new Column("OrderId", DbType.Guid, ColumnProperty.NotNull),
                new Column("OrderNumber", DbType.String, ColumnProperty.Null),
                new Column("ShippingNumber", DbType.String, ColumnProperty.Null),
                new Column("ShippingWarehouseId", DbType.Guid, ColumnProperty.Null),
                new Column("DeliveryWarehouseId", DbType.Guid, ColumnProperty.Null),
                new Column("ShippingDate", DbType.DateTime, ColumnProperty.Null),
                new Column("DeliveryDate", DbType.DateTime, ColumnProperty.Null),
                new Column("PalletsCount", DbType.Int32, ColumnProperty.Null),
                new Column("WeightKg", DbType.Decimal, ColumnProperty.Null),
                new Column("Route", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null),
                new Column("TarifficationType", DbType.Int32, ColumnProperty.Null),
                new Column("AutogroupingType", DbType.Int32, ColumnProperty.Null),
                new Column("BestCost", DbType.Decimal, ColumnProperty.Null),
                new Column("FtlDirectCost", DbType.Decimal, ColumnProperty.Null),
                new Column("FtlRouteCost", DbType.Decimal, ColumnProperty.Null),
                new Column("LtlCost", DbType.Decimal, ColumnProperty.Null),
                new Column("PoolingCost", DbType.Decimal, ColumnProperty.Null),
                new Column("MilkrunCost", DbType.Decimal, ColumnProperty.Null),
                new Column("CreatedAt", DbType.DateTime, ColumnProperty.NotNull));
        }
    }
}
