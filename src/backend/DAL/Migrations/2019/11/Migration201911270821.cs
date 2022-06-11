using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911270821)]
    public class AddShippingAvisationTime : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ShippingAvisationTime", DbType.Time, ColumnProperty.Null));
        }
    }
}
