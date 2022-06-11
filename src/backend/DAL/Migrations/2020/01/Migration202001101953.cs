using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202001101953)]
    public class AddNewColumnsToWarehouses : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Warehouses", new Column("PoolingId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("Warehouses", new Column("DistributionCenterId", DbType.String.WithSize(255)));
        }
    }
}
