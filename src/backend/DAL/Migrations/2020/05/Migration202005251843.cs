using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202005251843)]
    public class Migration202005251843 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("LoadingDowntimeCost", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("UnloadingDowntimeCost", DbType.Decimal, ColumnProperty.Null));

            Database.AddColumn("TransportCompanies", new Column("LoadingDowntimeRate", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("TransportCompanies", new Column("UnloadingDowntimeRate", DbType.Decimal, ColumnProperty.Null));
            Database.AddColumn("TransportCompanies", new Column("StandardLoadingTime", DbType.Decimal, ColumnProperty.Null));
        }
    }
}
