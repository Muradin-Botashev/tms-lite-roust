using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202001241430)]
    public class Migration202001241430 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("DriverPassportData", DbType.String, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("DriverName", DbType.String.WithSize(255), ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("DriverPhone", DbType.String.WithSize(255), ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("VehicleNumber", DbType.String.WithSize(255), ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("VehicleMake", DbType.String.WithSize(255), ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("TrailerNumber", DbType.String.WithSize(255), ColumnProperty.Null));
        }
    }
}
