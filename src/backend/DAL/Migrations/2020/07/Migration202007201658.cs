using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202007201658)]
    public class Migration202007201658 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Companies", new Column("NewShippingTarifficationType", DbType.Int32, ColumnProperty.Null));
        }
    }
}
