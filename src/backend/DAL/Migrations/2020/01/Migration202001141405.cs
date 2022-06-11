using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202001141405)]
    public class AddNewColumnsToOrders2 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("DeliveryAccountNumber", DbType.String.WithSize(255)));
            Database.AddColumn("Orders",new Column("DocumentAttached", DbType.Boolean, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("AmountConfirmed", DbType.Boolean, ColumnProperty.Null));
        }
    }
}
