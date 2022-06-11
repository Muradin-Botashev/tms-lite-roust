using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202011181335)]
    public class Migration202011181335 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("ShippingSchedules",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("ShippingCity", DbType.String, ColumnProperty.Null),
                new Column("DeliveryCity", DbType.String, ColumnProperty.Null),
                new Column("CarrierId", DbType.Guid, ColumnProperty.Null));

            Database.ExecuteNonQuery($@"
                ALTER TABLE ""ShippingSchedules""
                ADD COLUMN ""ShippingDays"" integer[];

                ALTER TABLE ""ShippingSchedules""
                ADD COLUMN ""DeliveryDays"" integer[];
            ");
        }
    }
}
