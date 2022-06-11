using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202001290931)]
    public class Migration202001290931 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("ShippingWarehouses", new Column("PoolingId", DbType.String, ColumnProperty.Null));
            Database.AddColumn("TransportCompanies", new Column("PoolingId", DbType.String, ColumnProperty.Null));
        }
    }
}
