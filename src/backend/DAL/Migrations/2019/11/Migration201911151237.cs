using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911151237)]
    public class AddOrderManualFields : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ManualShippingDate", DbType.Boolean, defaultValue: false));
            Database.AddColumn("Orders", new Column("ManualDeliveryDate", DbType.Boolean, defaultValue: false));
            Database.AddColumn("Orders", new Column("ManualPickingTypeId", DbType.Boolean, defaultValue: false));
        }
    }
}
