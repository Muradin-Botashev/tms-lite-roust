using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202006051548)]
    public class Migration202006051548 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("AutogroupingOrders", new Column("CarrierId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("AutogroupingOrders", new Column("VehicleTypeId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("AutogroupingOrders", new Column("BodyTypeId", DbType.Guid, ColumnProperty.Null));
        }
    }
}
