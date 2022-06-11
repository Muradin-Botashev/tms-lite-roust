using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202003012145)]
    public class Migration202003012145 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("StatusChangedAt", DbType.DateTime, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("StatusChangedAt", DbType.DateTime, ColumnProperty.Null));
        }
    }
}
