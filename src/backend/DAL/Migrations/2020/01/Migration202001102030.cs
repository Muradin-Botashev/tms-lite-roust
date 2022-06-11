using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202001102030)]
    public class AddNewColumnsToDics : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("BodyTypes", new Column("PoolingId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("ShippingWarehouses", new Column("PoolingRegionId", DbType.String.WithSize(255)));
        }
    }
}
