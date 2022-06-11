using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202001161610)]
    public class Migration202001161610 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("DriverName", DbType.String.WithSize(255)));
            Database.AddColumn("Orders", new Column("DriverPhone", DbType.String.WithSize(11)));
            Database.AddColumn("Orders", new Column("VehicleNumber", DbType.String.WithSize(255)));
        }
    }
}
