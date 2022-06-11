using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911221329)]
    public class AddDeliveryTypeToWarehouses : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Warehouses", new Column("DeliveryType", DbType.Int32, ColumnProperty.Null));
        }
    }
}
