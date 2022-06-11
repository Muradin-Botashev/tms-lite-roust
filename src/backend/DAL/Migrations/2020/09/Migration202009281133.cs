using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202009281133)]
    public class Migration202009281133 : Migration
    {
        public override void Apply()
        {
            Database.RemoveColumn("AutogroupingSettings", "AutogroupingTypes");
        }
    }
}
