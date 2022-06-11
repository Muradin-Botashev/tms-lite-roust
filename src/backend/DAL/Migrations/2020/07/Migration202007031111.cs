using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202007031111)]
    public class Migration202007031111 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Companies", new Column("PoolingProductType", DbType.Int32, ColumnProperty.Null));
            Database.AddColumn("Companies", new Column("PoolingToken", DbType.String.WithSize(500), ColumnProperty.Null));
        }
    }
}
