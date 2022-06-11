using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202007011104)]
    public class Migration202007011104 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Warehouses", new Column("Client", DbType.String.WithSize(255), ColumnProperty.Null));

            Database.ExecuteNonQuery(@"UPDATE ""Warehouses"" SET ""Client"" = ""WarehouseName""");
        }
    }
}
