using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202005211755)]
    public class Migration202005211755 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("AutogroupingSettings", new Column("CheckPoolingSlots", DbType.Boolean, ColumnProperty.Null));
            Database.AddColumn("AutogroupingSettings", new Column("TonnageId", DbType.Guid, ColumnProperty.Null));
        }
    }
}
