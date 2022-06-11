using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911261430)]
    public class Migration201911261430 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ManualDeliveryCost", DbType.Boolean, defaultValue: false));
        }
    }
}
