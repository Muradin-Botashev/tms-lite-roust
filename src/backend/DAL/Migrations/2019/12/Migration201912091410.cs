using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201912091410)]
    public class AddManualClientAvisationTime : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ManualClientAvisationTime", DbType.Boolean, defaultValue: false));
        }
    }
}
