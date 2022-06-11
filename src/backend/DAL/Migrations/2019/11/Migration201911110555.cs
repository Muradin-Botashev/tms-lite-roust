using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911110555)]
    public class AddActionsToRole : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery(@"
                ALTER TABLE ""Roles"" 
                ADD COLUMN ""Actions"" text[];

                UPDATE ""Roles""
                SET ""Actions"" = '{""createShipping"",""cancelOrder"",""removeFromShipping"",""sendToArchive"",""recordFactOfLoss"",""orderShipped"",""orderDelivered"",""fullReject"",""deleteOrder"",""sendShippingToTk"",""confirmShipping"",""rejectRequestShipping"",""cancelRequestShipping"",""completeShipping"",""cancelShipping"",""problemShipping"",""billingShipping"",""archiveShipping""}';
            ");
        }
    }
}
