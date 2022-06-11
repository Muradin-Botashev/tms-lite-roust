using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202004081158)]
    public class Migration202004081158 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("CarrierRequestDatesStats",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("ShippingId", DbType.Guid, ColumnProperty.NotNull),
                new Column("CarrierId", DbType.Guid, ColumnProperty.NotNull),
                new Column("SentAt", DbType.DateTime, ColumnProperty.Null),
                new Column("RejectedAt", DbType.DateTime, ColumnProperty.Null),
                new Column("ConfirmedAt", DbType.DateTime, ColumnProperty.Null));

            Database.ExecuteNonQuery($@"
				insert into ""CarrierRequestDatesStats"" (""Id"", ""ShippingId"", ""CarrierId"", ""SentAt"", ""RejectedAt"", ""ConfirmedAt"")
				select md5(random()::text || clock_timestamp()::text)::uuid,
					""Id"",
					""CarrierId"",
					(select MAX(""CreatedAt"")
					 from ""HistoryEntries""
					 where ""HistoryEntries"".""PersistableId"" = ""Shippings"".""Id""
	 					and ""MessageKey"" = 'shippingSetRequestSent'),
					(select MAX(""CreatedAt"")
					 from ""HistoryEntries""
					 where ""HistoryEntries"".""PersistableId"" = ""Shippings"".""Id""
	 					and ""MessageKey"" = 'shippingSetRejected'
						and ""CreatedAt"" > (select MAX(""CreatedAt"")
										   from ""HistoryEntries""
										   where ""HistoryEntries"".""PersistableId"" = ""Shippings"".""Id""
												and ""MessageKey"" = 'shippingSetRequestSent')),
					(select MAX(""CreatedAt"")
					 from ""HistoryEntries""
					 where ""HistoryEntries"".""PersistableId"" = ""Shippings"".""Id""
	 					and ""MessageKey"" = 'shippingSetConfirmed'
						and ""CreatedAt"" > (select MAX(""CreatedAt"")
										   from ""HistoryEntries""
										   where ""HistoryEntries"".""PersistableId"" = ""Shippings"".""Id""
												and ""MessageKey"" = 'shippingSetRequestSent'))
				from ""Shippings""
				where ""CarrierId"" is not null
			");
        }
    }
}
