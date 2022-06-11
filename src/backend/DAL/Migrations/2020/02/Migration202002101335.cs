using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202002101335)]
    public class ChangePoolingIdType : Migration
    {
        public override void Apply()
        {
            Database.ChangeColumn("Warehouses", "PoolingId", DbType.String, false);
            Database.ChangeColumn("BodyTypes", "PoolingId", DbType.String, false);
        }
    }
}
