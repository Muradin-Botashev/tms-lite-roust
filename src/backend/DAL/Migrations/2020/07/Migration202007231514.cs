using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202007231514)]
    public class Migration202007231514 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("ExtraPointCostsWithoutVAT", DbType.Decimal, ColumnProperty.Null));
        }
    }
}
