using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202001271015)]
    public class Migration202001271015 : Migration
    {
        public override void Apply()
        {
            Database.ChangeColumn("Orders", "DriverPhone", DbType.String.WithSize(50), false);
        }
    }
}
