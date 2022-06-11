using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202002271420)]
    public class Migration202002271420 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Warehouses", new Column("ShippingAvisationTime", DbType.Time, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("ManualShippingAvisationTime", DbType.Boolean, defaultValue: false));
        }
    }
}
