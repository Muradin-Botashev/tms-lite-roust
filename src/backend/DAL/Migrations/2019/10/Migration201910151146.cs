using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201910151146)]
    public class RenameUpperCaseColumns : Migration
    {
        public override void Apply()
        {
            Database.RenameColumn("OrderItems", "SPGR", "Spgr");
            Database.RenameColumn("Articles", "SPGR", "Spgr");
            Database.RenameColumn("Articles", "EANShrink", "EanShrink");
            Database.RenameColumn("Articles", "EANBox", "EanBox");
            Database.RenameColumn("Articles", "EANPallet", "EanPallet");
        }
    }
}
