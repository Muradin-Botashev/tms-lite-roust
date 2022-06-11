using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202001201935)]
    public class Migration202001201935 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("LoadingArrivalDate", DbType.Date, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("LoadingDepartureDate", DbType.Date, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("UnloadingArrivalDate", DbType.Date, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("UnloadingDepartureDate", DbType.Date, ColumnProperty.Null));

            Database.ExecuteNonQuery(@"update ""Orders"" SET 
                ""LoadingArrivalDate"" = ""LoadingArrivalTime""::date,
                ""LoadingDepartureDate"" = ""LoadingDepartureTime""::date,
                ""UnloadingArrivalDate"" = ""UnloadingArrivalTime""::date,
                ""UnloadingDepartureDate"" = ""UnloadingDepartureTime""::date;");

            Database.ChangeColumn("Orders", "LoadingArrivalTime", DbType.Time, false);
            Database.ChangeColumn("Orders", "LoadingDepartureTime", DbType.Time, false);
            Database.ChangeColumn("Orders", "UnloadingArrivalTime", DbType.Time, false);
            Database.ChangeColumn("Orders", "UnloadingDepartureTime", DbType.Time, false);
        }
    }
}
