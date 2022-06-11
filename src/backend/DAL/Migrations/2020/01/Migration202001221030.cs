using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202001221030)]
    public class Migration202001221030 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("IsPooling", DbType.Boolean, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("IsPooling", DbType.Boolean, ColumnProperty.Null));
        }
    }
}
