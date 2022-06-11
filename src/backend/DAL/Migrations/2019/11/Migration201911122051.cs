using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911122051)]
    public class AddTransportCompaniesIsActiveColumn : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("TransportCompanies", new Column("IsActive", DbType.Boolean, defaultValue: true));
        }
    }
}
