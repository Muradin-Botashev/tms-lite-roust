using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201911111502)]
    public class AddBodyTypeTable : Migration
    {
        public override void Apply()
        {
            Database.AddTable("BodyTypes",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(255)),
                new Column("IsActive", DbType.Boolean, defaultValue: true));
        }
    }
}
