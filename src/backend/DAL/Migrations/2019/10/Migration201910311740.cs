using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201910311740)]
    public class AddOrderChangeDateColumn : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("OrderChangeDate", DbType.DateTime, ColumnProperty.Null));
        }
    }
}
