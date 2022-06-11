using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201911190655)]
    public class AddAddressFieldsToWarehouses : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Warehouses", new Column("PostalCode", DbType.String.WithSize(100)));
            Database.AddColumn("Warehouses", new Column("Area", DbType.String.WithSize(255)));
            Database.AddColumn("Warehouses", new Column("Street", DbType.String.WithSize(255)));
            Database.AddColumn("Warehouses", new Column("House", DbType.String.WithSize(100)));
            Database.AddColumn("Warehouses", new Column("ValidAddress", DbType.String.WithSize(500)));
            Database.AddColumn("Warehouses", new Column("UnparsedAddressParts", DbType.String.WithSize(500)));
            Database.AddColumn("Warehouses", new Column("IsActive", DbType.Boolean, defaultValue: true));
        }
    }
}
