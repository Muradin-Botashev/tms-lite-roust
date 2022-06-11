using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201911111512)]
    public class AddBTonnageTable : Migration
    {
        public override void Apply()
        {
            Database.AddTable("Tonnages",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(255)),
                new Column("IsActive", DbType.Boolean, defaultValue: true));
        }
    }
}
