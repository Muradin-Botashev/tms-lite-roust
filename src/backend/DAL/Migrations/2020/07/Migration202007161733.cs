using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202007161733)]
    public class Migration202007161733 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("ShippingDate", DbType.DateTime, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("DeliveryDate", DbType.DateTime, ColumnProperty.Null));

            Database.ExecuteNonQuery($@"
                update ""Shippings""
                set ""ShippingDate"" = (select MIN(""Orders"".""ShippingDate"") from ""Orders"" where ""Orders"".""ShippingId"" = ""Shippings"".""Id""),
                    ""DeliveryDate"" = (select MAX(""Orders"".""DeliveryDate"") from ""Orders"" where ""Orders"".""ShippingId"" = ""Shippings"".""Id"")
            ");
        }
    }
}
