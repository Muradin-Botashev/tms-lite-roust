using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202007221300)]
    public class Migration202007221300 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Companies", new Column("OrderRequiresConfirmation", DbType.Boolean, ColumnProperty.Null));

            Database.ExecuteNonQuery($@"
                UPDATE ""Companies"" SET ""OrderRequiresConfirmation"" = true 
            ");
        }
    }
}
