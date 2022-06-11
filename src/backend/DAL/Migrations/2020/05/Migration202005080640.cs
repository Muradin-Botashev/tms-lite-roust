using System;
using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(202005080640)]
    public class Migration202005080640 : Migration
    {
        public override void Apply()
        {
            Database.AddTable("Companies",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(255)),
                new Column("IsActive", DbType.Boolean, ColumnProperty.NotNull, defaultValue: true));

            Database.AddColumn("Articles", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("BodyTypes", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("DocumentTypes", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("Orders", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("PickingTypes", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("Shippings", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("ShippingWarehouses", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("Tonnages", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("TransportCompanies", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("VehicleTypes", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));
            Database.AddColumn("Warehouses", new Column("CompanyId", DbType.Guid, ColumnProperty.Null));

            string companyId = Guid.NewGuid().ToString("D");

            Database.ExecuteNonQuery($@"
                ALTER TABLE ""Users"" ADD COLUMN ""CompanyIds"" UUID[];

                INSERT INTO ""Companies"" (""Id"", ""Name"", ""IsActive"") VALUES ('{companyId}', 'Company', true);

                UPDATE ""Articles"" SET ""CompanyId"" = '{companyId}';
                UPDATE ""BodyTypes"" SET ""CompanyId"" = '{companyId}';
                UPDATE ""DocumentTypes"" SET ""CompanyId"" = '{companyId}';
                UPDATE ""Orders"" SET ""CompanyId"" = '{companyId}';
                UPDATE ""PickingTypes"" SET ""CompanyId"" = '{companyId}';
                UPDATE ""Shippings"" SET ""CompanyId"" = '{companyId}';
                UPDATE ""ShippingWarehouses"" SET ""CompanyId"" = '{companyId}';
                UPDATE ""Tonnages"" SET ""CompanyId"" = '{companyId}';
                UPDATE ""TransportCompanies"" SET ""CompanyId"" = '{companyId}';
                UPDATE ""VehicleTypes"" SET ""CompanyId"" = '{companyId}';
                UPDATE ""Warehouses"" SET ""CompanyId"" = '{companyId}';

                UPDATE ""Users"" SET ""CompanyIds"" = '{{""{companyId}""}}';
            ");
        }
    }
}
