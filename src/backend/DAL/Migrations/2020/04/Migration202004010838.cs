using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202004010838)]
    public class Migration202004010838 : Migration
    {
        public override void Apply()
        {
            Database.ChangeColumn("Orders", "ShippingDate", DbType.DateTime, false);
            Database.ChangeColumn("Orders", "DeliveryDate", DbType.DateTime, false);
            Database.ChangeColumn("Orders", "LoadingArrivalDate", DbType.DateTime, false);
            Database.ChangeColumn("Orders", "LoadingDepartureDate", DbType.DateTime, false);
            Database.ChangeColumn("Orders", "UnloadingArrivalDate", DbType.DateTime, false);
            Database.ChangeColumn("Orders", "UnloadingDepartureDate", DbType.DateTime, false);

            Database.ExecuteNonQuery($@"
                update ""Orders""
                set ""ShippingDate"" = ""ShippingDate"" + ""ShippingAvisationTime""
                where ""ShippingDate"" is not null and ""ShippingAvisationTime"" is not null;

                update ""Orders""
                set ""DeliveryDate"" = ""DeliveryDate"" + ""ClientAvisationTime""
                where ""DeliveryDate"" is not null and ""ClientAvisationTime"" is not null;

                update ""Orders""
                set ""LoadingArrivalDate"" = ""LoadingArrivalDate"" + ""LoadingArrivalTime""
                where ""LoadingArrivalDate"" is not null and ""LoadingArrivalTime"" is not null;

                update ""Orders""
                set ""LoadingDepartureDate"" = ""LoadingDepartureDate"" + ""LoadingDepartureTime""
                where ""LoadingDepartureDate"" is not null and ""LoadingDepartureTime"" is not null;

                update ""Orders""
                set ""UnloadingArrivalDate"" = ""UnloadingArrivalDate"" + ""UnloadingArrivalTime""
                where ""UnloadingArrivalDate"" is not null and ""UnloadingArrivalTime"" is not null;

                update ""Orders""
                set ""UnloadingDepartureDate"" = ""UnloadingDepartureDate"" + ""UnloadingDepartureTime""
                where ""UnloadingDepartureDate"" is not null and ""UnloadingDepartureTime"" is not null;
            ");

            Database.RemoveColumn("Warehouses", "AvisaleTime");
            Database.RemoveColumn("Warehouses", "ShippingAvisationTime");

            Database.RemoveColumn("Orders", "ShippingAvisationTime");
            Database.RemoveColumn("Orders", "ManualShippingAvisationTime");
            Database.RemoveColumn("Orders", "ClientAvisationTime");
            Database.RemoveColumn("Orders", "ManualClientAvisationTime");
            Database.RemoveColumn("Orders", "LoadingArrivalTime");
            Database.RemoveColumn("Orders", "LoadingDepartureTime");
            Database.RemoveColumn("Orders", "UnloadingArrivalTime");
            Database.RemoveColumn("Orders", "UnloadingDepartureTime");

            Database.RenameColumn("Orders", "LoadingArrivalDate", "LoadingArrivalTime");
            Database.RenameColumn("Orders", "LoadingDepartureDate", "LoadingDepartureTime");
            Database.RenameColumn("Orders", "UnloadingArrivalDate", "UnloadingArrivalTime");
            Database.RenameColumn("Orders", "UnloadingDepartureDate", "UnloadingDepartureTime");
        }
    }
}
