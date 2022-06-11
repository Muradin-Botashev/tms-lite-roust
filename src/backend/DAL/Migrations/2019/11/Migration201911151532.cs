using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201911151532)]
    public class AddFieldPropertyVisibilityItems : Migration
    {
        public override void Apply()
        {
            Database.AddTable("FieldPropertyVisibilityItems",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("ForEntity", DbType.Int32),
                new Column("CompanyId", DbType.Guid, ColumnProperty.Null),
                new Column("RoleId", DbType.Guid, ColumnProperty.Null),
                new Column("FieldName", DbType.String.WithSize(255)),
                new Column("IsHidden", DbType.Boolean));
        }
    }
}
