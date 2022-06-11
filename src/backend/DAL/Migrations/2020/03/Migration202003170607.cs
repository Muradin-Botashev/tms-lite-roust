using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202003170607)]
    public class Migration202003170607 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("ActualTotalDeliveryCostWithoutVAT", DbType.Decimal, ColumnProperty.Null));
        }
    }
}
