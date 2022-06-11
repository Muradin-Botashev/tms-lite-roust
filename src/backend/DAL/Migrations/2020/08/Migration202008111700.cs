using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202008111700)]
    public class Migration202008111700 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("AutogroupingSettings", new Column("RegionOverrunCoefficient", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("AutogroupingSettings", new Column("InterregionOverrunCoefficient", DbType.Decimal, ColumnProperty.Null));

            Database.AddColumn("ShippingWarehouses", new Column("GeoQuality", DbType.Int32, ColumnProperty.Null));
            Database.AddColumn("Warehouses", new Column("GeoQuality", DbType.Int32, ColumnProperty.Null));
        }
    }
}
