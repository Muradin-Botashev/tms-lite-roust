using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202006091728)]
    public class Migration202006091728 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("AutogroupingOrders", new Column("DeliveryRegion", DbType.String.WithSize(255), ColumnProperty.Null));
            Database.AddColumn("AutogroupingOrders", new Column("DeliveryTime", DbType.Time, ColumnProperty.Null));
        }
    }
}
