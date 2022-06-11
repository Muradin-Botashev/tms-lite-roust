using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201912031340)]
    public class ExtendOrderSource : Migration
    {
        public override void Apply()
        {
            Database.ChangeColumn("Orders", "Source", new ColumnType(DbType.String, int.MaxValue), false);
        }
    }
}
