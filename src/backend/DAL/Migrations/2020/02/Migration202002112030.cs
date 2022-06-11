using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202002112030)]
    public class NewShippingsColumns : Migration
    {
        public override void Apply()
        {
            Database.RenameColumn("Shippings", "TotalDeliveryCost", "TotalDeliveryCostWithoutVAT");
            Database.RenameColumn("Shippings", "ManualTotalDeliveryCost", "ManualTotalDeliveryCostWithoutVAT");

            Database.AddColumn("Shippings", new Column("TotalDeliveryCost", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("BasicDeliveryCostWithoutVAT", DbType.Decimal, ColumnProperty.Null));
        }
    }
}
