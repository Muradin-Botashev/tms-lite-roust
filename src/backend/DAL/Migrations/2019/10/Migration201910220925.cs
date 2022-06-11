using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201910220925)]
    public class AddTimeToUnloadingDates : Migration
    {
        public override void Apply()
        {
            Database.ChangeColumn("Orders", "UnloadingArrivalTime", DbType.DateTime, false);
            Database.ChangeColumn("Orders", "UnloadingDepartureTime", DbType.DateTime, false);
        }
    }
}
