using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201912171554)]
    public class ClearNullRolePropertyVisibilitySettings : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery(@"
                DELETE FROM ""FieldPropertyVisibilityItems""
                WHERE ""RoleId"" IS null;");
        }
    }
}