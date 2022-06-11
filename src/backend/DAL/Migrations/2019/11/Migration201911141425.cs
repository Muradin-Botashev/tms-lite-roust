using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911141425)]
    public class AddOrderItemManual : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("OrderItems", new Column("IsManualEdited", DbType.Boolean, defaultValue: false));
        }
    }
}
