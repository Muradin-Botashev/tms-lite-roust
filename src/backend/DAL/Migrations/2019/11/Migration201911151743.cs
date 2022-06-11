using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911151743)]
    public class AddUserCarrierIdColumn : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Users", new Column("CarrierId", DbType.Guid, property: ColumnProperty.Null));
        }
    }
}
