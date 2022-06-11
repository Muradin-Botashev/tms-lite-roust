using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202001280836)]
    public class AddNotificationEvents : Migration
    {
        public override void Apply()
        {
            Database.AddTable("NotificationEvents",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("EntityId", DbType.Guid),
                new Column("Type", DbType.Int32),
                new Column("CreatedAt", DbType.DateTime),
                new Column("IsProcessed", DbType.Boolean));
        }
    }
}
