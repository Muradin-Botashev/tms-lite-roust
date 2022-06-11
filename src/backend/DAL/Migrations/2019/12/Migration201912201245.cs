using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201912201245)]
    public class RemoveCustomerWarehouseFromWarehouses : Migration
    {
        public override void Apply()
        {
            Database.RemoveColumn("Warehouses", "CustomerWarehouse");
        }
    }
}