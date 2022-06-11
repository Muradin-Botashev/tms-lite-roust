using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations._2020
{
    [Migration(202007011008)]
    public class Migration202007011008 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Tariffs", new Column("ShipmentRegion", DbType.String, ColumnProperty.Null));
            Database.AddColumn("Tariffs", new Column("DeliveryRegion", DbType.String, ColumnProperty.Null));
        }
    }
}
