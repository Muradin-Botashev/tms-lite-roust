using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201911070637)]
    public class AddShippingWarehouses : Migration
    {
        public override void Apply()
        {
            Database.AddTable("ShippingWarehouses",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Code", DbType.String.WithSize(255)),
                new Column("WarehouseName", DbType.String.WithSize(255)),
                new Column("Address", DbType.String.WithSize(500)),
                new Column("ValidAddress", DbType.String.WithSize(500)),
                new Column("PostalCode", DbType.String.WithSize(100)),
                new Column("Region", DbType.String.WithSize(255)),
                new Column("Area", DbType.String.WithSize(255)),
                new Column("City", DbType.String.WithSize(255)),
                new Column("Street", DbType.String.WithSize(255)),
                new Column("House", DbType.String.WithSize(100)),
                new Column("IsActive", DbType.Boolean, defaultValue: true));
        }
    }
}
