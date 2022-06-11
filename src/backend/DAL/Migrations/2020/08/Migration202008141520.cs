using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202008141520)]
    public class Migration202008141520 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Tariffs", new Column("PoolingPalletRate", DbType.Decimal, ColumnProperty.Null));
        }
    }
}
