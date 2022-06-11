using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201910111150)]
    public class AddShippingNumberToOrder : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ShippingNumber", DbType.String.WithSize(100), ColumnProperty.Null));
        }
    }
}
