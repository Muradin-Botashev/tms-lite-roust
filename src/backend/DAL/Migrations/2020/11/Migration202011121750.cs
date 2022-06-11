using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202011121750)]
    public class Migration202011121750 : Migration
    {
        public override void Apply()
        {
            Database.RemoveColumn("TransportCompanies", "ContractNumber");
            Database.RemoveColumn("TransportCompanies", "DateOfPowerOfAttorney");
            Database.RemoveColumn("TransportCompanies", "LoadingDowntimeRate");
            Database.RemoveColumn("TransportCompanies", "UnloadingDowntimeRate");
            Database.RemoveColumn("TransportCompanies", "StandardLoadingTime");

            Database.AddColumn("TransportCompanies", new Column("PowerOfAttorneyNumber", DbType.String, ColumnProperty.Null));
            Database.AddColumn("TransportCompanies", new Column("DateOfPowerOfAttorney", DbType.Date, ColumnProperty.Null));
            Database.AddColumn("TransportCompanies", new Column("Email", DbType.String, ColumnProperty.Null));
            Database.AddColumn("TransportCompanies", new Column("ContactInfo", DbType.String, ColumnProperty.Null));
            Database.AddColumn("TransportCompanies", new Column("Forwarder", DbType.String, ColumnProperty.Null));
            Database.AddColumn("TransportCompanies", new Column("RequestReviewDuration", DbType.Int32, ColumnProperty.Null));
        }
    }
}
