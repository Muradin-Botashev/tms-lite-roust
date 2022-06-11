using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202002141148)]
    public class Migration202002141148 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("PoolingReservationId", DbType.String, ColumnProperty.Null));
        }
    }
}
