using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202009281633)]
    public class Migration202009281633 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("AutogroupingCosts",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("AutogroupingShippingId", DbType.Guid, ColumnProperty.NotNull),
                new Column("CarrierId", DbType.Guid, ColumnProperty.NotNull),
                new Column("AutogroupingType", DbType.Int32, ColumnProperty.NotNull),
                new Column("Value", DbType.Decimal, ColumnProperty.Null),
                new Column("CreatedAt", DbType.DateTime, ColumnProperty.NotNull));
        }
    }
}
