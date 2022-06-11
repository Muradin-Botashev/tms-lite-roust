using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202005221605)]
    public class Migration202005221605 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Tariffs", new Column("ShippingWarehouseId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("Tariffs", new Column("DeliveryWarehouseId", DbType.Guid, ColumnProperty.Null));
        }
    }
}
