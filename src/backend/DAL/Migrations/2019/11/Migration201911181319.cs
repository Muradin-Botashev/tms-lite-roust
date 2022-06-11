using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201911181319)]
    public class AddSourceToOrder : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("Source", DbType.String.WithSize(500)));
        }
    }
}
