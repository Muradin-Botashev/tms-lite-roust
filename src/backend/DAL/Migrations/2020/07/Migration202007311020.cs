using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202007311020)]
    public class Migration202007311020 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("ShippingWarehouseId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("DeliveryWarehouseId", DbType.Guid, ColumnProperty.Null));

            Database.ExecuteNonQuery($@"
                UPDATE ""Shippings""
                SET ""ShippingWarehouseId"" = (
                        SELECT ""Orders"".""ShippingWarehouseId"" 
                        FROM ""Orders""
                        WHERE ""Orders"".""ShippingId"" = ""Shippings"".""Id""
                        ORDER BY ""Orders"".""ShippingDate""
                        LIMIT 1
                    ),
                    ""DeliveryWarehouseId"" = (
                        SELECT ""Orders"".""DeliveryWarehouseId"" 
                        FROM ""Orders""
                        WHERE ""Orders"".""ShippingId"" = ""Shippings"".""Id""
                        ORDER BY ""Orders"".""DeliveryDate"" desc
                        LIMIT 1
                    )
            ");
        }
    }
}
