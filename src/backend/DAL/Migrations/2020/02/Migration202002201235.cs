using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202002201235)]
    public class Migration202002201235 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("SyncedWithPooling", DbType.Boolean, ColumnProperty.NotNull, false));
        }
    }
}
