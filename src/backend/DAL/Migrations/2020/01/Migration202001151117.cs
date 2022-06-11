using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202001151117)]
    public class Migration202001151117 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("MasterPasswords",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Hash", DbType.String),
                new Column("CreatedAt", DbType.DateTime),
                new Column("AuthorId", DbType.Guid),
                new Column("IsActive", DbType.Boolean));
        }
    }
}
