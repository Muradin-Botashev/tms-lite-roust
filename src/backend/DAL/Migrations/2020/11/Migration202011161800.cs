using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202011161800)]
    public class Migration202011161800 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("InboundFiles",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Type", DbType.String, ColumnProperty.Null),
                new Column("RawContent", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null),
                new Column("ParsedContent", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null),
                new Column("UserId", DbType.Guid, ColumnProperty.Null),
                new Column("ReceivedAtUtc", DbType.DateTime, ColumnProperty.NotNull));
        }
    }
}
