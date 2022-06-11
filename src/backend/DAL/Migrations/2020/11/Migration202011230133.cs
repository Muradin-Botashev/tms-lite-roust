using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202011230133)]
    public class Migration202011230133 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("LeadTimes",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("ClientName", DbType.String, ColumnProperty.Null),
                new Column("DeliveryAddress", DbType.String, ColumnProperty.Null),
                new Column("ShippingWarehouseId", DbType.Guid, ColumnProperty.Null),
                new Column("LeadtimeDays", DbType.Int32, ColumnProperty.Null));
            Database.AddIndex("leadTimes_pk", true, "LeadTimes", "Id");
        }
    }
}