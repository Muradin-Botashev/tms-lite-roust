using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201910111330)]
    public class AddShippingCreationDate : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("ShippingCreationDate", DbType.DateTime));
        }
    }
}
