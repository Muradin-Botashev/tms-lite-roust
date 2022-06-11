using System.Data;
using ThinkingHome.Migrator.Framework;

namespace DAL.Migrations
{
    [Migration(202011181900)]
    public class Migration202011181900 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("Drivers",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String, ColumnProperty.Null),
                new Column("DriverLicence", DbType.String, ColumnProperty.Null),
                new Column("Passport", DbType.String, ColumnProperty.Null),
                new Column("Phone", DbType.String, ColumnProperty.Null),
                new Column("Email", DbType.String, ColumnProperty.Null),
                new Column("IsBlackList", DbType.Boolean, ColumnProperty.Null),
                new Column("IsActive", DbType.Boolean, ColumnProperty.Null));
        }
    }
}