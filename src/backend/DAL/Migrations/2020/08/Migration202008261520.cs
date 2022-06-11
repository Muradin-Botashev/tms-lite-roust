using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202008261520)]
    public class Migration202008261520 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("ShippingAddress", DbType.String.WithSize(1000), ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("DeliveryAddress", DbType.String.WithSize(1000), ColumnProperty.Null));

            Database.ExecuteNonQuery($@"
                UPDATE ""Shippings""
                SET ""ShippingAddress"" = (
                        SELECT ""Orders"".""ShippingAddress"" 
                        FROM ""Orders""
                        WHERE ""Orders"".""ShippingId"" = ""Shippings"".""Id""
                        ORDER BY ""Orders"".""ShippingDate""
                        LIMIT 1
                    ),
                    ""DeliveryAddress"" = (
                        SELECT ""Orders"".""DeliveryAddress"" 
                        FROM ""Orders""
                        WHERE ""Orders"".""ShippingId"" = ""Shippings"".""Id""
                        ORDER BY ""Orders"".""DeliveryDate"" desc
                        LIMIT 1
                    )
            ");
        }
    }
}
