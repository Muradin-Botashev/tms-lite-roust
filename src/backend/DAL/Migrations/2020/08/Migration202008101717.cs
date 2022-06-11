using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202008101717)]
    public class Migration202008101717 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("NotificationEvents", new Column("Data", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null));
        }
    }
}
