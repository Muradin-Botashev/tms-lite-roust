using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202005211035)]
    public class Migration202005211035 : Migration
    {
        public override void Apply()
        {
            if (Database.ConstraintExists("Orders", "Orders_OrderNumber_UC"))
            {
                Database.RemoveConstraint("Orders", "Orders_OrderNumber_UC");
            }

            Database.AddUniqueConstraint("Orders_OrderNumber_UC", "Orders", "OrderNumber", "CompanyId");
        }
    }
}
