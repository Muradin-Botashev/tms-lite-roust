using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202005120656)]
    public class Migration202005120656 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Users", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("Roles", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));

            Database.ExecuteNonQuery($@"
                ALTER TABLE ""Users"" DROP COLUMN IF EXISTS ""CompanyIds"";

                UPDATE ""Users"" SET ""CompanyId"" = (SELECT ""Id"" FROM ""Companies"" LIMIT 1);
                UPDATE ""Roles"" SET ""CompanyId"" = (SELECT ""Id"" FROM ""Companies"" LIMIT 1);
            ");
        }
    }
}
