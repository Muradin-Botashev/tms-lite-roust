using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201912100919)]
    public class AddVehicleTypeIdToOrders : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("VehicleTypeId", DbType.Guid, ColumnProperty.Null));
        }
    }
}