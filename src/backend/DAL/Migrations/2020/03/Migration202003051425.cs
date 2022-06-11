using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202003051425)]
    public class Migration202003051425 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ManualBodyTypeId", DbType.Boolean, ColumnProperty.NotNull, false));
        }
    }
}
