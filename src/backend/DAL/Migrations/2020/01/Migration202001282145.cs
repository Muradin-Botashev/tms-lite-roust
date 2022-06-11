using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202001282145)]
    public class MIgration202001282145 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("SlotId", DbType.String.WithSize(255)));
            Database.AddColumn("Shippings", new Column("ConsolidationDate", DbType.DateTime, ColumnProperty.Null));
        }
    }
}
