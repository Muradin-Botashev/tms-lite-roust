using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202007211956)]
    public class Migration202007211956 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("CostsComments", DbType.String.WithSize(1000), ColumnProperty.Null));
        }
    }
}
