using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201901101935)]
    public class AddBookingNumberToOrders : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("BookingNumber", DbType.String.WithSize(255)));
        }
    }
}
