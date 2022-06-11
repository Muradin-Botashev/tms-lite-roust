using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911151635)]
    public class AddShippingBodyTypeColumn : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("BodyTypeId", DbType.Guid, property: ColumnProperty.Null));
        }
    }
}
