using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202003120930)]
    public class Migration202003120930 : Migration
    {
        public override void Apply()
        {
            Database.RemoveColumn("Shippings", "ManualTotalDeliveryCostWithoutVAT");
        }
    }
}
