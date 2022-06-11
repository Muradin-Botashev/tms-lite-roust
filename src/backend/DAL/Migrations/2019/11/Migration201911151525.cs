using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911151525)]
    public class AddTariffDates : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Tariffs", new Column("EffectiveDate", DbType.DateTime, property: ColumnProperty.Null));
            Database.AddColumn("Tariffs", new Column("ExpirationDate", DbType.DateTime, property: ColumnProperty.Null));
        }
    }
}
