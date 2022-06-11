using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201910111125)]
    public class RemoveUserPickingType : Migration
    {
        public override void Apply()
        {
            Database.RemoveColumn("Warehouses", "UsePickingType");
        }
    }
}
