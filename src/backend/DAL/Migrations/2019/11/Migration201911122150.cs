using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911122150)]
    public class AddPickingFaeturesColumn : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Warehouses", new Column("PickingFeatures", DbType.String));
        }
    }
}
