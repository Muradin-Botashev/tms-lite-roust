using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201921191550)]
    public class AddBodyTypeIdToTariffs : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Tariffs", new Column("BodyTypeId", DbType.Guid));
        }
    }
}
