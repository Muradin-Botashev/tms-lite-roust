using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202007231033)]
    public class Migration202007231033 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("ShippingWarehouses", new Column("PoolingConsolidationId", DbType.String, ColumnProperty.Null));
        }
    }
}
