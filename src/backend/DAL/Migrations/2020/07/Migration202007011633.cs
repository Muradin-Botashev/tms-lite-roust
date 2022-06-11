using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202007011633)]
    public class Migration202007011633 : Migration
    {
        public override void Apply()
        {
            Database.ChangeColumn("Orders", "PalletsCount", DbType.Decimal, false);
            Database.ChangeColumn("Orders", "ConfirmedPalletsCount", DbType.Decimal, false);
            Database.ChangeColumn("Orders", "ActualPalletsCount", DbType.Decimal, false);

            Database.ChangeColumn("AutogroupingOrders", "PalletsCount", DbType.Decimal, false);
        }
    }
}
