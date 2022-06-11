using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201910280642)]
    public class AddFieldPropertyItems : Migration
    {
        public override void Apply()
        {
            Database.AddTable("FieldPropertyItems",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("ForEntity", DbType.Int32),
                new Column("CompanyId", DbType.Guid, ColumnProperty.Null),
                new Column("RoleId", DbType.Guid, ColumnProperty.Null),
                new Column("FieldName", DbType.String.WithSize(255)),
                new Column("State", DbType.Int32),
                new Column("AccessType", DbType.Int32));
        }
    }
}
