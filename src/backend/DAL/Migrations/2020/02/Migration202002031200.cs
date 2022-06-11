using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202002031200)]
    public class AddIsNewCarrierRequest : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("IsNewCarrierRequest", DbType.Boolean, ColumnProperty.Null, defaultValue: false));
            Database.AddColumn("Shippings", new Column("IsNewCarrierRequest", DbType.Boolean, ColumnProperty.Null, defaultValue: false));

            Database.ExecuteNonQuery(@"
                ALTER TABLE ""Roles""
                ADD COLUMN ""Backlights"" integer[];
            ");
        }
    }
}
