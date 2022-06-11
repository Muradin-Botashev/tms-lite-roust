using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201910141233)]
    public class AddShippingColumnsToOrders : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("WaybillTorg12", DbType.Boolean, defaultValue: false));
            Database.AddColumn("Orders", new Column("Invoice", DbType.Boolean, defaultValue: false));
            Database.AddColumn("Orders", new Column("DocumentsReturnDate", DbType.DateTime, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("ActualDocumentsReturnDate", DbType.DateTime, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("OrderShippingStatus", DbType.Int32, ColumnProperty.Null));
        }
    }
}
