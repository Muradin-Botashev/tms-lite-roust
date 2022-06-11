using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202001151154)]
    public class Migration202001151154 : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery($@"
                update ""HistoryEntries""
                set ""MessageKey"" = 'documentAttachedMessage'
                where ""MessageKey"" = 'documentAttached'
            ");
        }
    }
}
