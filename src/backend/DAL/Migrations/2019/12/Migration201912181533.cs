using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201912181533)]
    public class AddManualTarifficationTypeInShipping : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Shippings", new Column("ManualTarifficationType", DbType.Boolean, defaultValue: false));
        }
    }
}