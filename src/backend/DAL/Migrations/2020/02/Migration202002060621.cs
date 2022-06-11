using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202002060621)]
    public class Migration202002060621 : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery($@"
                update ""Roles""
                set ""Backlights"" = '{{1}}'
                where ""Name"" = 'TransportCoordinator'
            ");
        }
    }
}
