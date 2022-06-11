using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(201911122050)]
    public class AddDocumentTypesIsActiveColumn : Migration
    {
        public override void Apply()
        {
            Database.AddColumn("DocumentTypes", new Column("IsActive", DbType.Boolean, defaultValue: true));
        }
    }
}
