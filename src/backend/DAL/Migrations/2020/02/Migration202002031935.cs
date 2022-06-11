using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202002031935)]
    public class AddBodyTypeRequest : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("BodyTypeId", DbType.Guid, ColumnProperty.Null));
        }
    }
}
