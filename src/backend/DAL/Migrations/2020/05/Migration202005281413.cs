using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202005281413)]
    public class Migration202005281413 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("AutogroupingOrders", new Column("FtlDirectCostMessage", DbType.String.WithSize(1000), ColumnProperty.Null));
            Database.AddColumn("AutogroupingOrders", new Column("FtlRouteCostMessage", DbType.String.WithSize(1000), ColumnProperty.Null));
            Database.AddColumn("AutogroupingOrders", new Column("LtlCostMessage", DbType.String.WithSize(1000), ColumnProperty.Null));
            Database.AddColumn("AutogroupingOrders", new Column("MilkrunCostMessage", DbType.String.WithSize(1000), ColumnProperty.Null));
            Database.AddColumn("AutogroupingOrders", new Column("PoolingCostMessage", DbType.String.WithSize(1000), ColumnProperty.Null));
        }
    }
}
