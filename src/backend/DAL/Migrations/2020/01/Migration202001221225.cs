using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202001221225)]
    public class ClearTaskProperties : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery(@" delete from ""TaskProperties"" ");
        }
    }
}
