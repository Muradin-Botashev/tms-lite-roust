using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202003041430)]
    public class Migration202003041430 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("PoolingWarehouseId", DbType.String, ColumnProperty.Null));
        }
    }
}
