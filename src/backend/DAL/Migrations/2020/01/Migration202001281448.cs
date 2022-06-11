using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202001281448)]
    public class AddShippingWarehouseState : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ShippingWarehouseState", DbType.Int32, defaultValue: 0));
        }
    }
}
