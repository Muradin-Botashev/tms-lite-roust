using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace DAL.Migrations
{
    [Migration(201910081000)]
    public class InitDatabaseScheme : Migration
    {
        public override void Apply()
        {
            Database.AddTable("Roles",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(100)));
            Database.AddIndex("Roles_pk", true, "Roles", "Id");

            Database.AddTable("Users",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(100)),
                new Column("RoleId", DbType.Guid),
                new Column("Email", DbType.String.WithSize(100)),
                new Column("IsActive", DbType.Boolean, defaultValue: true),
                new Column("FieldsConfig", DbType.String.WithSize(300)),
                new Column("PasswordHash", DbType.String.WithSize(300)));
            Database.AddIndex("Users_pk", true, "Users", "Id");

            Database.AddTable("Translations",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(100)),
                new Column("En", DbType.String.WithSize(255)),
                new Column("Ru", DbType.String.WithSize(255)));
            Database.AddIndex("Translations_pk", true, "Translations", "Id");

            Database.AddTable("Injections",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Type", DbType.String.WithSize(100)),
                new Column("FileName", DbType.String.WithSize(200)),
                new Column("Status", DbType.String.WithSize(10)),
                new Column("ProcessTimeUtc", DbType.DateTime));
            Database.AddIndex("Injections_pk", true, "Injections", "Id");

            Database.AddTable("TaskProperties",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("TaskName", DbType.String.WithSize(100)),
                new Column("Properties", DbType.String.WithSize(1000)));
            Database.AddIndex("TaskProperties_pk", true, "TaskProperties", "Id");

            Database.AddTable("Orders",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Status", DbType.Int32, ColumnProperty.Null),
                new Column("OrderNumber", DbType.String.WithSize(100)),
                new Column("OrderDate", DbType.DateTime, ColumnProperty.Null),
                new Column("OrderType", DbType.Int32, ColumnProperty.Null),
                new Column("Payer", DbType.String.WithSize(100)),
                new Column("ClientName", DbType.String.WithSize(255)),
                new Column("SoldTo", DbType.String.WithSize(100)),
                new Column("TemperatureMin", DbType.Int32, ColumnProperty.Null),
                new Column("TemperatureMax", DbType.Int32, ColumnProperty.Null),
                new Column("ShippingDate", DbType.DateTime, ColumnProperty.Null),
                new Column("TransitDays", DbType.Int32, ColumnProperty.Null),
                new Column("DeliveryDate", DbType.DateTime, ColumnProperty.Null),
                new Column("BDFInvoiceNumber", DbType.String.WithSize(100)),
                new Column("InvoiceNumber", DbType.String.WithSize(100)),
                new Column("ArticlesCount", DbType.Int32, ColumnProperty.Null),
                new Column("BoxesCount", DbType.Int32, ColumnProperty.Null),
                new Column("ConfirmedBoxesCount", DbType.Int32, ColumnProperty.Null),
                new Column("PalletsCount", DbType.Int32, ColumnProperty.Null),
                new Column("ConfirmedPalletsCount", DbType.Int32, ColumnProperty.Null),
                new Column("ActualPalletsCount", DbType.Int32, ColumnProperty.Null),
                new Column("WeightKg", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("ActualWeightKg", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("OrderAmountExcludingVAT", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("InvoiceAmountExcludingVAT", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("DeliveryRegion", DbType.String.WithSize(255)),
                new Column("DeliveryCity", DbType.String.WithSize(255)),
                new Column("ShippingAddress", DbType.String.WithSize(500)),
                new Column("DeliveryAddress", DbType.String.WithSize(500)),
                new Column("ShippingStatus", DbType.Int32, ColumnProperty.Null),
                new Column("DeliveryStatus", DbType.Int32, ColumnProperty.Null),
                new Column("ClientAvisationTime", DbType.Time, ColumnProperty.Null),
                new Column("OrderComments", DbType.String.WithSize(1000)),
                new Column("PickingTypeId", DbType.Guid, ColumnProperty.Null),
                new Column("PlannedArrivalTimeSlotBDFWarehouse", DbType.String.WithSize(255)),
                new Column("LoadingArrivalTime", DbType.DateTime, ColumnProperty.Null),
                new Column("LoadingDepartureTime", DbType.DateTime, ColumnProperty.Null),
                new Column("UnloadingArrivalTime", DbType.Date, ColumnProperty.Null),
                new Column("UnloadingDepartureTime", DbType.Date, ColumnProperty.Null),
                new Column("TrucksDowntime", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("ReturnInformation", DbType.String.WithSize(1000)),
                new Column("ReturnShippingAccountNo", DbType.String.WithSize(100)),
                new Column("PlannedReturnDate", DbType.DateTime, ColumnProperty.Null),
                new Column("ActualReturnDate", DbType.DateTime, ColumnProperty.Null),
                new Column("MajorAdoptionNumber", DbType.String.WithSize(100)),
                new Column("OrderCreationDate", DbType.DateTime, ColumnProperty.Null),
                new Column("ShippingId", DbType.Guid, ColumnProperty.Null),
                new Column("ShippingWarehouseId", DbType.Guid, ColumnProperty.Null),
                new Column("DeliveryWarehouseId", DbType.Guid, ColumnProperty.Null));
            Database.AddIndex("Orders_pk", true, "Orders", "Id");
            Database.AddIndex("Orders_Shipping_fk", false, "Orders", "ShippingId");
            Database.AddIndex("Orders_ShippingWarehouse_fk", false, "Orders", "ShippingWarehouseId");
            Database.AddIndex("Orders_DeliveryWarehouse_fk", false, "Orders", "DeliveryWarehouseId");

            Database.AddTable("OrderItems",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("OrderId", DbType.Guid),
                new Column("Nart", DbType.String.WithSize(100)),
                new Column("Quantity", DbType.Int32));
            Database.AddIndex("OrderItems_pk", true, "OrderItems", "Id");
            Database.AddIndex("OrderItems_Order_fk", false, "OrderItems", "OrderId");

            Database.AddTable("Shippings",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("ShippingNumber", DbType.String.WithSize(100)),
                new Column("DeliveryType", DbType.Int32, ColumnProperty.Null),
                new Column("TemperatureMin", DbType.Int32, ColumnProperty.Null),
                new Column("TemperatureMax", DbType.Int32, ColumnProperty.Null),
                new Column("TarifficationType", DbType.Int32, ColumnProperty.Null),
                new Column("CarrierId", DbType.Guid, ColumnProperty.Null),
                new Column("VehicleTypeId", DbType.Guid, ColumnProperty.Null),
                new Column("PalletsCount", DbType.Int32, ColumnProperty.Null),
                new Column("ManualPalletsCount", DbType.Boolean, defaultValue: false),
                new Column("ActualPalletsCount", DbType.Int32, ColumnProperty.Null),
                new Column("ManualActualPalletsCount", DbType.Boolean, defaultValue: false),
                new Column("ConfirmedPalletsCount", DbType.Int32, ColumnProperty.Null),
                new Column("ManualConfirmedPalletsCount", DbType.Boolean, defaultValue: false),
                new Column("WeightKg", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("ManualWeightKg", DbType.Boolean, defaultValue: false),
                new Column("ActualWeightKg", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("ManualActualWeightKg", DbType.Boolean, defaultValue: false),
                new Column("PlannedArrivalTimeSlotBDFWarehouse", DbType.String.WithSize(255)),
                new Column("LoadingArrivalTime", DbType.DateTime, ColumnProperty.Null),
                new Column("LoadingDepartureTime", DbType.DateTime, ColumnProperty.Null),
                new Column("DeliveryInvoiceNumber", DbType.String.WithSize(100)),
                new Column("DeviationReasonsComments", DbType.String.WithSize(1000)),
                new Column("TotalDeliveryCost", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("ManualTotalDeliveryCost", DbType.Boolean, defaultValue: false),
                new Column("OtherCosts", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("DeliveryCostWithoutVAT", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("ReturnCostWithoutVAT", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("InvoiceAmountWithoutVAT", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("AdditionalCostsWithoutVAT", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("AdditionalCostsComments", DbType.String.WithSize(1000)),
                new Column("TrucksDowntime", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("ManualTrucksDowntime", DbType.Boolean, defaultValue: false),
                new Column("ReturnRate", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("AdditionalPointRate", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("DowntimeRate", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("BlankArrivalRate", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("BlankArrival", DbType.Boolean, defaultValue: false),
                new Column("Waybill", DbType.Boolean, defaultValue: false),
                new Column("WaybillTorg12", DbType.Boolean, defaultValue: false),
                new Column("TransportWaybill", DbType.Boolean, defaultValue: false),
                new Column("Invoice", DbType.Boolean, defaultValue: false),
                new Column("DocumentsReturnDate", DbType.DateTime, ColumnProperty.Null),
                new Column("ActualDocumentsReturnDate", DbType.DateTime, ColumnProperty.Null),
                new Column("InvoiceNumber", DbType.String.WithSize(100)),
                new Column("Status", DbType.Int32, ColumnProperty.Null),
                new Column("CostsConfirmedByShipper", DbType.Boolean, defaultValue: false),
                new Column("CostsConfirmedByCarrier", DbType.Boolean, defaultValue: false));
            Database.AddIndex("Shippings_pk", true, "Shippings", "Id");

            Database.AddTable("Tariffs",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("ShipmentCity", DbType.String.WithSize(255)),
                new Column("DeliveryCity", DbType.String.WithSize(255)),
                new Column("TarifficationType", DbType.Int32, ColumnProperty.Null),
                new Column("CarrierId", DbType.Guid, ColumnProperty.Null),
                new Column("VehicleTypeId", DbType.Guid, ColumnProperty.Null),
                new Column("FTLRate", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate1", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate2", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate3", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate4", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate5", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate6", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate7", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate8", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate9", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate10", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate11", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate12", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate13", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate14", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate15", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate16", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate17", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate18", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate19", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate20", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate21", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate22", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate23", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate24", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate25", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate26", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate27", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate28", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate29", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate30", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate31", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate32", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null),
                new Column("LTLRate33", DbType.Decimal.WithSize(19, 5), ColumnProperty.Null));
            Database.AddIndex("Tariffs_pk", true, "Tariffs", "Id");

            Database.AddTable("Warehouses",
                new Column("WarehouseName", DbType.String.WithSize(255)),
                new Column("SoldToNumber", DbType.String.WithSize(100)),
                new Column("Region", DbType.String.WithSize(255)),
                new Column("City", DbType.String.WithSize(255)),
                new Column("Address", DbType.String.WithSize(500)),
                new Column("PickingTypeId", DbType.Guid, ColumnProperty.Null),
                new Column("LeadtimeDays", DbType.Int32, ColumnProperty.Null),
                new Column("CustomerWarehouse", DbType.Boolean, defaultValue: true),
                new Column("UsePickingType", DbType.Boolean, defaultValue: false),
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey));
            Database.AddIndex("warehouses_pk", true, "Warehouses", "Id");

            Database.AddTable("Articles",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("SPGR", DbType.String.WithSize(100), ColumnProperty.Null),
                new Column("Description", DbType.String.WithSize(255), ColumnProperty.Null),
                new Column("Nart", DbType.String.WithSize(100)),
                new Column("CountryOfOrigin", DbType.String.WithSize(255), ColumnProperty.Null),
                new Column("ShelfLife", DbType.Int32, ColumnProperty.Null),
                new Column("Status", DbType.String.WithSize(100)),
                new Column("Ean", DbType.String.WithSize(100), ColumnProperty.Null),
                new Column("UnitLengthGoodsMm", DbType.Int32, ColumnProperty.Null),
                new Column("WidthUnitsGoodsMm", DbType.Int32, ColumnProperty.Null),
                new Column("UnitHeightGoodsMm", DbType.Int32, ColumnProperty.Null),
                new Column("WeightUnitsGrossProductG", DbType.Int32, ColumnProperty.Null),
                new Column("WeightUnitsNetGoodsG", DbType.Int32, ColumnProperty.Null),
                new Column("EANShrink", DbType.String.WithSize(100), ColumnProperty.Null),
                new Column("PiecesInShrink", DbType.Int32, ColumnProperty.Null),
                new Column("LengthShrinkMm", DbType.Int32, ColumnProperty.Null),
                new Column("WidthShrinkMm", DbType.Int32, ColumnProperty.Null),
                new Column("HeightShrinkMm", DbType.Int32, ColumnProperty.Null),
                new Column("GrossShrinkWeightG", DbType.Int32, ColumnProperty.Null),
                new Column("NetWeightShrinkG", DbType.Int32, ColumnProperty.Null),
                new Column("EANBox", DbType.String.WithSize(100), ColumnProperty.Null),
                new Column("PiecesInABox", DbType.Int32, ColumnProperty.Null),
                new Column("BoxLengthMm", DbType.Int32, ColumnProperty.Null),
                new Column("WidthOfABoxMm", DbType.Int32, ColumnProperty.Null),
                new Column("BoxHeightMm", DbType.Int32, ColumnProperty.Null),
                new Column("GrossBoxWeightG", DbType.Int32, ColumnProperty.Null),
                new Column("NetBoxWeightG", DbType.Int32, ColumnProperty.Null),
                new Column("PiecesInALayer", DbType.Int32, ColumnProperty.Null),
                new Column("LayerLengthMm", DbType.Int32, ColumnProperty.Null),
                new Column("LayerWidthMm", DbType.Int32, ColumnProperty.Null),
                new Column("LayerHeightMm", DbType.Int32, ColumnProperty.Null),
                new Column("GrossLayerWeightMm", DbType.Int32, ColumnProperty.Null),
                new Column("NetWeightMm", DbType.Int32, ColumnProperty.Null),
                new Column("EANPallet", DbType.String.WithSize(100), ColumnProperty.Null),
                new Column("PiecesOnAPallet", DbType.Int32, ColumnProperty.Null),
                new Column("PalletLengthMm", DbType.Int32, ColumnProperty.Null),
                new Column("WidthOfPalletsMm", DbType.Int32, ColumnProperty.Null),
                new Column("PalletHeightMm", DbType.Int32, ColumnProperty.Null),
                new Column("GrossPalletWeightG", DbType.Int32, ColumnProperty.Null),
                new Column("NetWeightPalletsG", DbType.Int32, ColumnProperty.Null));
            Database.AddIndex("Articles_pk", true, "Articles", "Id");

            Database.AddTable("TransportCompanies",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Title", DbType.String.WithSize(255)),
                new Column("ContractNumber", DbType.String.WithSize(255)),
                new Column("DateOfPowerOfAttorney", DbType.String.WithSize(255)));
            Database.AddIndex("TransportCompanies_pk", true, "TransportCompanies", "Id");

            Database.AddTable("VehicleTypes",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(255), ColumnProperty.NotNull));
            Database.AddIndex("VehicleTypes_pk", true, "VehicleTypes", "Id");

            Database.AddTable("PickingTypes",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(255), ColumnProperty.NotNull));
            Database.AddIndex("PickingTypes_pk", true, "PickingTypes", "Id");

            Database.AddTable("FileStorage",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(255), ColumnProperty.NotNull),
                new Column("Data", DbType.Binary, ColumnProperty.NotNull));
            Database.AddIndex("FileStorage_pk", true, "FileStorage", "Id");

            Database.AddTable("DocumentTypes",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(255), ColumnProperty.NotNull));
            Database.AddIndex("DocumentTypes_pk", true, "DocumentTypes", "Id");

            Database.AddTable("Documents",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(255), ColumnProperty.NotNull),
                new Column("PersistableId", DbType.Guid, ColumnProperty.NotNull),
                new Column("FileId", DbType.Guid, ColumnProperty.NotNull),
                new Column("TypeId", DbType.Guid, ColumnProperty.Null));
            Database.AddIndex("Documents_pk", true, "Documents", "Id");
            Database.AddIndex("Documents_PersistableId_ix", false, "Documents", "PersistableId");

            Database.AddTable("HistoryEntries",
                new Column("Id", DbType.Guid, ColumnProperty.PrimaryKey),
                new Column("PersistableId", DbType.Guid, ColumnProperty.NotNull),
                new Column("UserId", DbType.Guid, ColumnProperty.Null),
                new Column("UserName", DbType.String.WithSize(255)),
                new Column("CreatedAt", DbType.DateTime),
                new Column("MessageKey", DbType.String.WithSize(100)),
                new Column("MessageArgs", DbType.String.WithSize(int.MaxValue)));
            Database.AddIndex("HistoryEntries_pk", true, "HistoryEntries", "Id");
            Database.AddIndex("HistoryEntries_PersistableId_ix", false, "HistoryEntries", "PersistableId");
        }
    }
}
