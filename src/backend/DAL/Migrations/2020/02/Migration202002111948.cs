using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202002111948)]
    public class AddAvailableUntilToShipping : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("AvailableUntil", DbType.DateTime, ColumnProperty.Null));
        }
    }
}
