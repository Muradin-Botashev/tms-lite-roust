using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201912131633)]
    public class ClearNullRolePropertySettings : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery(@"
                DELETE FROM ""FieldPropertyItems""
                WHERE ""RoleId"" IS null;");

            Database.ExecuteNonQuery(@"
                DELETE FROM ""FieldPropertyVisibilityItems""
                WHERE ""RoleId"" IS null;");
        }
    }
}