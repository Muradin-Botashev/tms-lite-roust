using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202001131143)]
    public class AddLoginToUsers : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Users", new Column("Login", DbType.String));

            Database.ExecuteNonQuery($@" update ""Users"" set ""Login"" = ""Email"" ");
        }
    }
}
