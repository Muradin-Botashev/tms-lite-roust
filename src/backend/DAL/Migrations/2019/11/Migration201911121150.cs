using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911121150)]
    public class AddVehicleTypeColumns : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("VehicleTypes", new Column("TonnageId", DbType.Guid, property: ColumnProperty.Null));
            Database.AddColumn("VehicleTypes", new Column("BodyTypeId", DbType.Guid, property: ColumnProperty.Null));
            Database.AddColumn("VehicleTypes", new Column("PalletsCount", DbType.Int32, property: ColumnProperty.Null));
            Database.AddColumn("VehicleTypes", new Column("IsActive", DbType.Boolean, defaultValue: true));
        }
    }
}
