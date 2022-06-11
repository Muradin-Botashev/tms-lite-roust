using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911111650)]
    public class AddTariffsColumns : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Tariffs", new Column("StartWinterPeriod", DbType.DateTime, property: ColumnProperty.Null));
            Database.AddColumn("Tariffs", new Column("EndWinterPeriod", DbType.DateTime, property: ColumnProperty.Null));
            Database.AddColumn("Tariffs", new Column("WinterAllowance", DbType.Decimal, property: ColumnProperty.Null));
        }
    }
}
