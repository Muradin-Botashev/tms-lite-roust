using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202003061240)]
    public class Migration202003061240 : Migration
    {
        public override void Apply()
        {
            Database.RemoveColumn("Orders", "ManualTotalAmount");
            Database.RemoveColumn("Orders", "ManualTotalAmountNds");

        }
    }
}
