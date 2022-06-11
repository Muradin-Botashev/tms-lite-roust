using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202005211305)]
    public class Migration202005211305 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("AutogroupingSettings",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("CompanyId", DbType.Guid, ColumnProperty.Null),
                new Column("MaxUnloadingPoints", DbType.Int32, ColumnProperty.Null));
        }
    }
}
