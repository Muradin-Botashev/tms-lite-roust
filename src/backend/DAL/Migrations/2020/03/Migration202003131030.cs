using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202003131030)]
    public class Migration202003131030 : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery(@"UPDATE ""Orders"" o SET ""StatusChangedAt"" = 
                (SELECT MAX(""CreatedAt"") FROM ""HistoryEntries"" h
                WHERE ""MessageKey"" IN(
                'orderSetCancelled',
                'orderSetConfirmed',
                'orderSetFullReturn',
                'orderSetDelivered',
                'orderSetShipped',
                'orderSetLost',
                'orderRemovedFromShipping',
                'orderRollback',
                'orderSetArchived',
                'orderSetCreated',
                'orderCancellingShipping',
                'orderSetInShipping',
                'orderSentToPooling',
                'orderCreatedFromInjection') AND h.""PersistableId"" = o.""Id"")");

            Database.ExecuteNonQuery(@"UPDATE ""Shippings"" s SET ""StatusChangedAt"" = 
                (SELECT MAX(""CreatedAt"") FROM ""HistoryEntries"" h
                WHERE ""MessageKey"" IN(
                    'shippingSetCancelled',
                    'shippingSetArchived',
                    'shippingSetBillSend',
                    'shippingSetCancelledRequest',
                    'shippingSetCompleted',
                    'shippingSetConfirmed',
                    'shippingSetProblem',
                    'shippingSetRejected',
                    'shippingRollback',
                    'shippingSetRequestSent',
                    'shippingSetCreated',
                    'shippingAddOrdersResendRequest') AND h.""PersistableId"" = s.""Id"")");
        }
    }
}
