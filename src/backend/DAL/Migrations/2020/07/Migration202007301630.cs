using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202007301630)]
    public class Migration202007301630 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("PoolingProductType", DbType.Int32, ColumnProperty.Null));
        }
    }
}
