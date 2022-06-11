using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201910311951)]
    public class AddOrderConfirmedColumn : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("OrderConfirmed", DbType.Boolean, ColumnProperty.NotNull, false));
        }
    }
}
