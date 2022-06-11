using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201910251014)]
    public class AddRolePermissions : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery(@"
                UPDATE public.""Roles""
                SET ""Permissions"" = '{0}'
                WHERE ""Name"" = 'Administrator';");

            Database.ExecuteNonQuery(@"
                UPDATE public.""Roles""
                SET ""Permissions"" = '{1, 2, 4, 5, 6, 7, 10, 11, 12, 13, 14}'
                WHERE ""Name"" != 'Administrator';");
        }
    }
}
