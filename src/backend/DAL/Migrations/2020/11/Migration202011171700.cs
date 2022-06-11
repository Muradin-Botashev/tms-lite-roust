using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202011171700)]
    public class Migration202011171700 : Migration
    {
        public override void Apply()
        {
            Database.RemoveColumn("FixedDirections", "VehicleTypeId");

            Database.ExecuteNonQuery($@"
                ALTER TABLE ""FixedDirections"" 
                ADD COLUMN ""VehicleTypeIds"" uuid[];
            ");
        }
    }
}
