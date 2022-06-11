using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202004131047)]
    public class Migration202004131047 : Migration
    {
        public override void Apply()
        {
            Database.ChangeColumn("Orders", "DeliveryAddress", DbType.String.WithSize(1000), false);
            Database.ChangeColumn("Orders", "ShippingAddress", DbType.String.WithSize(1000), false);

            Database.ChangeColumn("ShippingWarehouses", "Address", DbType.String.WithSize(1000), false);
            Database.ChangeColumn("ShippingWarehouses", "ValidAddress", DbType.String.WithSize(1000), false);

            Database.ChangeColumn("Warehouses", "Address", DbType.String.WithSize(1000), false);
            Database.ChangeColumn("Warehouses", "ValidAddress", DbType.String.WithSize(1000), false);
            Database.ChangeColumn("Warehouses", "UnparsedAddressParts", DbType.String.WithSize(1000), false);
        }
    }
}
