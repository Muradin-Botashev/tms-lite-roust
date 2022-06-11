using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202007311800)]
    public class Migration202007311800 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("AutogroupingOrders", new Column("Errors", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null));
        }
    }
}
