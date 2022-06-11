using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911220638)]
    public class AddShippingCityToOrders : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ShippingCity", DbType.String));

            Database.ExecuteNonQuery($@"
                update ""Orders""
                set ""ShippingCity"" = (select ""City"" from ""ShippingWarehouses"" where ""ShippingWarehouseId"" = ""ShippingWarehouses"".""Id"")
                where ""ShippingWarehouseId"" is not null
            ");
        }
    }
}
