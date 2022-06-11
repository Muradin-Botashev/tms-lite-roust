using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201912091722)]
    public class AddTarifficationTypeIdToTariffs : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("TarifficationType", DbType.Int32, ColumnProperty.Null));
        }
    }
}