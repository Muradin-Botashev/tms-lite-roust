using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202006241141)]
    public class Migration202006241141 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("CityDistances",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("FromCity", DbType.String, ColumnProperty.NotNull),
                new Column("ToCity", DbType.String, ColumnProperty.NotNull),
                new Column("Distance", DbType.Decimal, ColumnProperty.Null));
        }
    }
}
