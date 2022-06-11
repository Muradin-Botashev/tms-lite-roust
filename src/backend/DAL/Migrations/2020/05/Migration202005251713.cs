using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202005251713)]
    public class Migration202005251713 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("Volume", DbType.Decimal, ColumnProperty.Null));
        }
    }
}
