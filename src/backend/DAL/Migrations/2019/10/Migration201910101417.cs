using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201910101417)]
    public class Migration201910101417 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("UserSettings",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("UserId", DbType.Guid),
                new Column("Key", DbType.String.WithSize(100)),
                new Column("Value", DbType.String.WithSize(int.MaxValue), ColumnProperty.Null));

            Database.AddIndex("UserSettings_pk", true, "UserSettings", "Id");
            Database.AddIndex("UserSettings_ix", false, "UserSettings", "UserId", "Key");
        }
    }
}
