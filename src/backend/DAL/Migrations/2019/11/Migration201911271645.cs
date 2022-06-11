using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201911271645)]
    public class AddAvisaleTimeToWarehouse : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Warehouses", new Column("AvisaleTime", DbType.Time, ColumnProperty.Null));
        }
    }
}
