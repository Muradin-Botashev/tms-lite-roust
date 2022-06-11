using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202001271348)]
    public class AddNotificationsToUsers : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery(@"
                ALTER TABLE ""Users""
                ADD COLUMN ""Notifications"" integer[];
            ");
        }
    }
}
