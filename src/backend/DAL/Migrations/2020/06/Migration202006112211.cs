using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202006112211)]
    public class Migration202006161042 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ShippingRegion", DbType.String, ColumnProperty.Null));

            Database.ExecuteNonQuery($@"
                update ""Orders""
                set ""ShippingRegion"" = (select ""Region"" from ""ShippingWarehouses"" where ""ShippingWarehouseId"" = ""ShippingWarehouses"".""Id"" limit 1)
                where ""ShippingWarehouseId"" is not null
            ");
        }
    }
}
