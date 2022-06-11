using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202008071700)]
    public class Migration202008071700 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("NotificationEvents", new Column("InitiatorId", DbType.Guid, ColumnProperty.Null));
        }
    }
}
