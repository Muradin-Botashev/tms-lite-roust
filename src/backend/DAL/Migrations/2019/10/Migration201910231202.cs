using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201910211254)]
    public class CreateRolePermissionsTable : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery(
                @"ALTER TABLE public.""Roles""
                ADD COLUMN ""Permissions"" integer[];");
        }
    }
}
