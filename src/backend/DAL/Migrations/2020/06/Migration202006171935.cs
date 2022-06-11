using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202006171935)]
    public class Migration202006171935 : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery(
                @"ALTER TABLE public.""AutogroupingSettings""
                ADD COLUMN ""AutogroupingTypes"" integer[];");
        }
    }
}
