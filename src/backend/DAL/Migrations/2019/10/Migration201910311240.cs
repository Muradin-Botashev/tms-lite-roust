using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201910311240)]
    public class ChangeOrderNumbers : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("ClientOrderNumber", DbType.String.WithSize(100)));

            Database.ExecuteNonQuery(@"
                update ""Orders"" set ""ClientOrderNumber""=""OrderNumber"";
                update ""Orders"" set ""OrderNumber""=""BdfInvoiceNumber"";
            ");

            Database.RemoveColumn("Orders", "BdfInvoiceNumber");
        }
    }
}
