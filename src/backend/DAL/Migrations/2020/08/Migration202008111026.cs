using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202008111026)]
    public class Migration202008111026 : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("VehicleTypes", new Column("IsInterregion", DbType.Boolean, ColumnProperty.Null));

            Database.AddColumn("AutogroupingShippings", new Column("BodyTypeId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("AutogroupingShippings", new Column("VehicleTypeId", DbType.Guid, ColumnProperty.Null));
        }
    }
}
