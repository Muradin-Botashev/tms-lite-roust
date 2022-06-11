using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202005201110)]
    public class Migration202005201110 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Tonnages", new Column("WeightKg", DbType.Decimal));

            Database.ExecuteNonQuery($@"
                update ""Tonnages""
                set ""WeightKg"" = CAST(REPLACE(SUBSTRING(""Name"", '[0-9,.]+'), ',', '.') as decimal) * 1000
            ");
        }
    }
}
