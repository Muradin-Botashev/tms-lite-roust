using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911121950)]
    public class AddPickingTypesIsActiveColumn : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("PickingTypes", new Column("IsActive", DbType.Boolean, defaultValue: true));
        }
    }
}
