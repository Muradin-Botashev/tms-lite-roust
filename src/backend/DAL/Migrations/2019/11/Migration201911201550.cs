using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201911201550)]
    public class AddPickingFeaturesToOrders : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("PickingFeatures", DbType.String.WithSize(255)));

            this.Database.ExecuteNonQuery(@"
                update ""Orders"" SET ""PickingFeatures"" = ""Warehouses"".""PickingFeatures""
                FROM ""Warehouses""
                Where ""Warehouses"".""SoldToNumber"" = ""Orders"".""SoldTo""
            ");
        }
    }
}
