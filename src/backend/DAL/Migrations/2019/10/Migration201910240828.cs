using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201910240828)]
    public class AddOrderManualPalletsCount : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ManualPalletsCount", DbType.Boolean, defaultValue: false));
            Database.RenameColumn("Orders", "BDFInvoiceNumber", "BdfInvoiceNumber");
        }
    }
}
