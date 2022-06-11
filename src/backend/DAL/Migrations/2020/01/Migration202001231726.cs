using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202001231726)]
    public class Migration202001231726 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("DriverPassportData", DbType.String, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("VehicleMake", DbType.String.WithSize(255), ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("TrailerNumber", DbType.String.WithSize(255), ColumnProperty.Null));
        }
    }
}
