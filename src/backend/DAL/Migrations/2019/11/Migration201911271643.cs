using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911271643)]
    public class AddRollbackActionsToRole : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery(@"
                
                UPDATE ""Roles""
                SET ""Actions"" = '{""createShipping"",""cancelOrder"",""removeFromShipping"",""sendToArchive"",""recordFactOfLoss"",""orderShipped"",""orderDelivered"",""fullReject"",""deleteOrder"",""sendShippingToTk"",""confirmShipping"",""rejectRequestShipping"",""cancelRequestShipping"",""completeShipping"",""cancelShipping"",""problemShipping"",""billingShipping"",""archiveShipping"",""rollbackOrder"",""rollbackShipping""}'
                Where ""Name""= 'Administrator';");
        }
    }
}