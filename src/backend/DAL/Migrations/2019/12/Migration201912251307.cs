using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201912251307)]
    public class AddIsNewForConfirmed : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("Orders", new Column("IsNewForConfirmed", DbType.Boolean, defaultValue: false));
        }
    }
}