using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201910141005)]
    public class AddArticleColumnsToOrderItems : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("OrderItems", new Column("Description", DbType.String.WithSize(255), ColumnProperty.Null));
            Database.AddColumn("OrderItems", new Column("CountryOfOrigin", DbType.String.WithSize(255), ColumnProperty.Null));
            Database.AddColumn("OrderItems", new Column("SPGR", DbType.String.WithSize(100), ColumnProperty.Null));
            Database.AddColumn("OrderItems", new Column("Ean", DbType.String.WithSize(100), ColumnProperty.Null));
            Database.AddColumn("OrderItems", new Column("ShelfLife", DbType.Int32, ColumnProperty.Null));
        }
    }
}
