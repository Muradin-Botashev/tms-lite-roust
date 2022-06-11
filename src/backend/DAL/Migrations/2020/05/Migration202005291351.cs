using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations._2020
{
    [Migration(202005291351)]
    public class Migration202005291351 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Tariffs", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));

            Database.ExecuteNonQuery($@"
                UPDATE ""Tariffs"" 
                SET ""CompanyId"" = (
                    SELECT ""CompanyId"" 
                    FROM ""TransportCompanies"" 
                    WHERE ""TransportCompanies"".""Id"" = ""Tariffs"".""CarrierId""
                )
            ");
        }
    }
}
