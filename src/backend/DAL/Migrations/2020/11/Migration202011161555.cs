using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202011161555)]
    public class Migration202011161555 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("CarrierShippingActions",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("CarrierId", DbType.Guid, ColumnProperty.NotNull),
                new Column("ShippingId", DbType.Guid, ColumnProperty.NotNull),
                new Column("ActionTime", DbType.DateTime, ColumnProperty.NotNull),
                new Column("ActionName", DbType.String, ColumnProperty.NotNull));
        }
    }
}
