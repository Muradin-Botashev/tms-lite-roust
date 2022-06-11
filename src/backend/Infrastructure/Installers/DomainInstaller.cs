using Application.BusinessModels.Articles.Triggers;
using Application.BusinessModels.OrderItems.Triggers;
using Application.BusinessModels.Orders.Actions;
using Application.BusinessModels.Orders.Backlights;
using Application.BusinessModels.Orders.Triggers;
using Application.BusinessModels.Orders.Validation;
using Application.BusinessModels.Shared.Actions;
using Application.BusinessModels.Shared.Backlights;
using Application.BusinessModels.Shared.Triggers;
using Application.BusinessModels.Shared.Validation;
using Application.BusinessModels.Shippings.Actions;
using Application.BusinessModels.Shippings.Triggers;
using Application.BusinessModels.Shippings.Validation;
using Application.BusinessModels.ShippingWarehouses.Triggers;
using Application.BusinessModels.Tariffs.Triggers;
using Application.BusinessModels.Warehouses.Triggers;
using Application.Services;
using Application.Services.AppConfiguration;
using Application.Services.Articles;
using Application.Services.Autogrouping;
using Application.Services.AutogroupingSettings;
using Application.Services.BodyTypes;
using Application.Services.Companies;
using Application.Services.Documents;
using Application.Services.DocumentTypes;
using Application.Services.Drivers;
using Application.Services.FieldProperties;
using Application.Services.Files;
using Application.Services.FixedDirections;
using Application.Services.History;
using Application.Services.Identity;
using Application.Services.Import;
using Application.Services.Injections;
using Application.Services.Leadtime;
using Application.Services.Orders;
using Application.Services.Orders.Import;
using Application.Services.OrderStates;
using Application.Services.PickingTypes;
using Application.Services.Pooling;
using Application.Services.Profile;
using Application.Services.Reports;
using Application.Services.Roles;
using Application.Services.Shippings;
using Application.Services.Shippings.Import;
using Application.Services.ShippingSchedules;
using Application.Services.ShippingWarehouseRegion;
using Application.Services.ShippingWarehouses;
using Application.Services.Tariffs;
using Application.Services.TaskProperties;
using Application.Services.Tonnages;
using Application.Services.Translations;
using Application.Services.TransportCompanies;
using Application.Services.Users;
using Application.Services.UserSettings;
using Application.Services.VehicleTypes;
using Application.Services.WarehouseAddress;
using Application.Services.WarehouseCity;
using Application.Services.Warehouses;
using Application.Services.Warehouses.Import;
using Application.Shared;
using Application.Shared.Addresses;
using Application.Shared.BodyTypes;
using Application.Shared.Distances;
using Application.Shared.Email;
using Application.Shared.Notifications;
using Application.Shared.Orders;
using Application.Shared.Pooling;
using Application.Shared.Shippings;
using Application.Shared.TransportCompanies;
using Application.Shared.Triggers;
using DAL;
using DAL.Services;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.AppConfiguration;
using Domain.Services.Articles;
using Domain.Services.Autogrouping;
using Domain.Services.AutogroupingSettings;
using Domain.Services.BodyTypes;
using Domain.Services.Companies;
using Domain.Services.Documents;
using Domain.Services.DocumentTypes;
using Domain.Services.Drivers;
using Domain.Services.FieldProperties;
using Domain.Services.Files;
using Domain.Services.FixedDirections;
using Domain.Services.History;
using Domain.Services.Identity;
using Domain.Services.Import;
using Domain.Services.Injections;
using Domain.Services.Leadtime;
using Domain.Services.Orders;
using Domain.Services.Orders.Import;
using Domain.Services.OrderStates;
using Domain.Services.PickingTypes;
using Domain.Services.Pooling;
using Domain.Services.Profile;
using Domain.Services.Reports;
using Domain.Services.Reports.Registry;
using Domain.Services.Roles;
using Domain.Services.Shippings;
using Domain.Services.Shippings.Import;
using Domain.Services.ShippingSchedules;
using Domain.Services.ShippingWarehouseCity;
using Domain.Services.ShippingWarehouseRegion;
using Domain.Services.ShippingWarehouses;
using Domain.Services.Tariffs;
using Domain.Services.TaskProperties;
using Domain.Services.Tonnages;
using Domain.Services.Translations;
using Domain.Services.TransportCompanies;
using Domain.Services.Users;
using Domain.Services.UserSettings;
using Domain.Services.VehicleTypes;
using Domain.Services.WarehouseAddress;
using Domain.Services.WarehouseCity;
using Domain.Services.WarehouseRegion;
using Domain.Services.Warehouses;
using Domain.Services.Warehouses.Import;
using Domain.Shared.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Infrastructure.Installers
{
    public static class DomainInstaller
    {
        public static void AddDomain(this IServiceCollection services, IConfiguration configuration, bool migrateDb)
        {
            services.AddSingleton(configuration);

            services.AddScoped<AppDbContext, AppDbContext>();
            services.AddScoped<IAppConfigurationService, AppConfigurationService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<ICompaniesService, CompaniesService>();
            services.AddScoped<ITranslationsService, TranslationsService>();
            services.AddScoped<IInjectionsService, InjectionsService>();
            services.AddScoped<ITaskPropertiesService, TaskPropertiesService>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddScoped<IUserSettingsService, UserSettingsService>();
            services.AddScoped<IAutogroupingSettingsService, AutogroupingSettingsService>();
            services.AddScoped<IShippingSchedulesService, ShippingSchedulesService>();
            services.AddScoped<IFixedDirectionsService, FixedDirectionsService>();

            services.AddScoped<IAutogroupingOpenService, AutogroupingOpenService>();
            services.AddScoped<IAutogroupingService, AutogroupingService>();
            services.AddScoped<IAutogroupingOrdersService, AutogroupingOrdersService>();
            services.AddScoped<IGroupingOrdersService, GroupingOrdersService>();
            services.AddScoped<IGroupCostCalculationService, GroupCostCalculationService>();

            services.AddScoped<IOpenImportService, OpenImportService>();

            services.AddScoped<ICommonDataService, CommonDataService>();
            services.AddScoped<IAuditDataService, AuditDataService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDeliveryCostCalcService, DeliveryCostCalcService>();
            services.AddScoped<IOrderFieldsSyncService, OrderFieldsSyncService>();
            services.AddScoped<IShippingTarifficationTypeDeterminer, ShippingTarifficationTypeDeterminer>();
            services.AddScoped<IShippingCalculationService, ShippingCalculationService>();

            services.AddScoped<ITriggersService, TriggersService>();

            /*start of add service implementation*/
            services.AddScoped<IOrdersService, OrdersService>();
            services.AddScoped<IOrderChangesService, OrderChangesService>();
            services.AddScoped<IOrderStateService, OrderStateService>();
            services.AddScoped<IShippingsService, ShippingsService>();
            services.AddScoped<ITariffsService, TariffsService>();
            services.AddScoped<IShippingWarehousesService, ShippingWarehousesService>();
            services.AddScoped<IWarehousesService, WarehousesService>();
            services.AddScoped<ISoldToService, SoldToService>();
            services.AddScoped<IClientNameService, ClientNameService>();
            services.AddScoped<IShippingWarehousesForOrderCreation, ShippingWarehousesForOrderCreation>();
            services.AddScoped<IArticlesService, ArticlesService>();
            services.AddScoped<ITransportCompaniesService, TransportCompaniesService>();
            services.AddScoped<IFilesService, FilesService>();
            services.AddScoped<IDefaultBodyTypeService, DefaultBodyTypeService>();
            services.AddScoped<IDocumentTypesService, DocumentTypesService>();
            services.AddScoped<IPickingTypesService, PickingTypesService>();
            services.AddScoped<IVehicleTypesService, VehicleTypesService>();
            services.AddScoped<IFieldPropertiesService, FieldPropertiesService>();
            services.AddSingleton<IFieldDispatcherService, FieldDispatcherService>();
            services.AddScoped<IBodyTypesService, BodyTypesService>();
            services.AddScoped<ITonnagesService, TonnagesService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<IOrderShippingStatusService, OrderShippingStatusService>();
            services.AddScoped<IShippingActionService, ShippingActionService>();
            services.AddScoped<IShippingChangesService, ShippingChangesService>();
            services.AddScoped<ISendShippingService, SendShippingService>();
            services.AddScoped<IDriverDataSyncService, DriverDataSyncService>();
            services.AddScoped<IHttpClientService, HttpClientService>();

            services.AddScoped<IOrderPoolingService, OrderPoolingService>();
            services.AddScoped<IPoolingApiService, PoolingApiService>();

            services.AddScoped<IInputReservationsService, InputReservationsService>();

            services.AddScoped<IWarehouseCityService, WarehouseCityService>();
            services.AddScoped<IWarehouseRegionService, WarehouseRegionService>();
            services.AddScoped<IWarehouseAddressService, WarehouseAddressService>();
            services.AddScoped<IShippingWarehouseCityService, ShippingWarehouseCityService>();
            services.AddScoped<IShippingWarehouseRegionService, ShippingWarehouseRegionService>();

            services.AddScoped<IOrdersImportService, OrdersImportService>();
            services.AddScoped<IPoolingWarehousesImportService, PoolingWarehousesImportService>();
            services.AddScoped<IShippingVehicleImportService, ShippingVehicleImportService>();
            services.AddScoped<IInvoicesImportService, InvoicesImportService>();

            services.AddScoped<ICleanAddressService, CleanAddressService>();
            services.AddScoped<IWarehouseDistancesService, WarehouseDistancesService>();
            services.AddScoped<IProfileService, ProfileService>();

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<ICarrierSelectionService, CarrierSelectionService>();

            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<IChangeTrackerFactory, ChangeTrackerFactory>();

            services.AddScoped<IReportService, OperationalReportService>();
            services.AddScoped<IRegistryReportService, RegistryReportService>();

            services.AddScoped<IDriversService, DriversService>();
            services.AddScoped<ILeadtimeService, LeadtimeService>();

            /*end of add service implementation*/

            AddOrderBusinessModels(services);
            AddShippingBusinessModels(services);
            AddDictionariesBusinessModels(services);

            InitDatabase(services, configuration, migrateDb);
        }

        private static void InitDatabase(IServiceCollection services, IConfiguration configuration, bool migrateDb)
        {
            var connectionString = configuration.GetConnectionString("DefaultDatabase");

            var buildServiceProvider = services.AddEntityFrameworkNpgsql()
                .AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                })
                .BuildServiceProvider();

            var appDbContext = buildServiceProvider.GetService<AppDbContext>();

            if (migrateDb)
            {
                appDbContext.Migrate(connectionString);
            }

            var shippingsCount = GetShippingCount(appDbContext);
            ShippingNumberProvider.InitLastNumber(shippingsCount);
        }

        private static int GetShippingCount(AppDbContext appDbContext)
        {
            var lastShipping = appDbContext.Shippings
                .Select(i => i.ShippingNumber)
                .OrderByDescending(i => i)
                .FirstOrDefault();

            if (lastShipping == null) return 0;

            var numberStr = lastShipping.Replace("SH", "").TrimStart('0');

            if (!int.TryParse(numberStr, out int number))
            {
                throw new System.Exception("Couldn't init last shipping number");
            }

            return number;
        }

        private static void AddOrderBusinessModels(IServiceCollection services)
        {
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, ActualPalletsCountReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, ActualWeightKgReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, BodyTypeIdReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, BoxesCountReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, Application.BusinessModels.Orders.Validation.CarrierIdCompanyValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, CarrierIdReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, Application.BusinessModels.Orders.Validation.CarrierIdValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, DateValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, DeliveryAddressReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, DeliveryCityValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, DeliveryDateReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, DeliveryRegionValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, DeliveryWarehouseIdReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, LoadingDateValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, OrderAmountExcludingVATReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, PalletsCountReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, PickingTypeIdCompanyValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, ShippingAddressReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, ShippingDateReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, ShippingWarehouseIdCompanyValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, ShippingWarehouseIdReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, SoldToReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, Application.BusinessModels.Orders.Validation.TarifficationTypeReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, UnloadingArrivalTimeValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, UnloadingDepartureTimeValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, Application.BusinessModels.Orders.Validation.VehicleTypeIdCompanyValidationRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, VehicleTypeIdReadonlyRule>();
            services.AddScoped<IValidationRule<Domain.Services.Orders.OrderDto, Order>, WeightKgReadonlyRule>();

            services.AddScoped<IAppAction<Order>, CreateShipping>();
            services.AddScoped<IAppAction<Order>, ConfirmOrder>();
            services.AddScoped<IAppAction<Order>, CancelOrder>();
            services.AddScoped<IAppAction<Order>, SendToArchive>();
            services.AddScoped<IAppAction<Order>, RecordFactOfLoss>();
            services.AddScoped<IAppAction<Order>, OrderShipped>();
            services.AddScoped<IAppAction<Order>, OrderDelivered>();
            services.AddScoped<IAppAction<Order>, FullReject>();
            services.AddScoped<IAppAction<Order>, DeleteOrder>();
            services.AddScoped<IAppAction<Order>, RollbackOrder>();

            services.AddScoped<IGroupAppAction<Order>, RemoveFromShipping>();
            services.AddScoped<IGroupAppAction<Order>, UnionOrders>();
            services.AddScoped<IGroupAppAction<Order>, UnionOrdersInExisted>();
            services.AddScoped<IGroupAppAction<Order>, UnitOrdersAndSendToTk>();
            services.AddScoped<IGroupAppAction<Order>, Application.BusinessModels.Orders.Actions.SendToPooling>();
            services.AddScoped<IGroupAppAction<Order>, Application.BusinessModels.Orders.Actions.CancelPoolingReservation>();
            services.AddScoped<IGroupAppAction<Order>, UnionOrdersInOtherShipping>();

            services.AddScoped<IAppAction<Order>, SendOrderShippingToTk>();
            services.AddScoped<IAppAction<Order>, ConfirmOrderShipping>();
            services.AddScoped<IAppAction<Order>, RejectRequestOrderShipping>();
            services.AddScoped<IAppAction<Order>, CancelRequestOrderShipping>();
            services.AddScoped<IAppAction<Order>, CompleteOrderShipping>();
            services.AddScoped<IAppAction<Order>, CancelOrderShipping>();
            services.AddScoped<IAppAction<Order>, ProblemOrderShipping>();
            services.AddScoped<IAppAction<Order>, BillingOrderShipping>();
            services.AddScoped<IAppAction<Order>, ArchiveOrderShipping>();
            services.AddScoped<IAppAction<Order>, RollbackOrderShipping>();

            services.AddScoped<ITrigger<OrderItem>, SyncItemProductData>();
            services.AddScoped<ITrigger<OrderItem>, UpdateItemQuantity>();

            services.AddScoped<ITrigger<Order>, CalcOrderCosts>();
            services.AddScoped<ITrigger<Order>, CalcOrderCreatedStatus>();
            services.AddScoped<ITrigger<Order>, CalcShippingTemperature>();
            services.AddScoped<ITrigger<Order>, CalcShippingTotalFields>();
            services.AddScoped<ITrigger<Order>, Application.BusinessModels.Orders.Triggers.SendUpdateShippingNotification>();
            services.AddScoped<ITrigger<Order>, Application.BusinessModels.Orders.Triggers.SyncBodyType>();
            services.AddScoped<ITrigger<Order>, SyncDeliveryOrderPointFields>();
            services.AddScoped<ITrigger<Order>, SyncDeliveryWarehouse>();
            services.AddScoped<ITrigger<Order>, Application.BusinessModels.Orders.Triggers.SyncShippingOrderFields>();
            services.AddScoped<ITrigger<Order>, SyncShippingOrderPointFields>();
            services.AddScoped<ITrigger<Order>, SyncShippingOrderTotals>();
            services.AddScoped<ITrigger<Order>, Application.BusinessModels.Orders.Triggers.SyncVehicleType>();
            services.AddScoped<ITrigger<Order>, UpdateBodyTypeToDefault>();
            services.AddScoped<ITrigger<Order>, Application.BusinessModels.Orders.Triggers.UpdateDeliveryAddress>();
            services.AddScoped<ITrigger<Order>, Application.BusinessModels.Orders.Triggers.UpdateManualFieldFlags>();
            services.AddScoped<ITrigger<Order>, UpdateOrderChangeDate>();
            services.AddScoped<ITrigger<Order>, UpdateOrderDeliveryCost>();
            services.AddScoped<ITrigger<Order>, UpdateOrderNumber>();
            services.AddScoped<ITrigger<Order>, UpdateOrderTotalCosts>();
            services.AddScoped<ITrigger<Order>, UpdatePalletsCount>();
            services.AddScoped<ITrigger<Order>, Application.BusinessModels.Orders.Triggers.UpdateShippingAddress>();
            services.AddScoped<ITrigger<Order>, UpdateShippingWarehouse>();
            services.AddScoped<ITrigger<Order>, UpdateStatus>();

            services.AddScoped<IValidationTrigger<Order>, Application.BusinessModels.Orders.Triggers.ValidateClearBacklightFlags>();
            services.AddScoped<IValidationTrigger<Order>, Application.BusinessModels.Orders.Triggers.ValidateSendChangesToPooling>();

            services.AddScoped<IBacklight<Order>, CarrierRequestSentBacklight>();
            services.AddScoped<IBacklight<Order>, OrderConfirmedBacklight>();
        }

        private static void AddShippingBusinessModels(IServiceCollection services)
        {
            services.AddScoped<IValidationRule<ShippingDto, Shipping>, BodyTypeIdCompanyValidationRule>();
            services.AddScoped<IValidationRule<ShippingDto, Shipping>, CarrierIdPoolingReadonlyRule>();
            services.AddScoped<IValidationRule<ShippingDto, Shipping>, Application.BusinessModels.Shippings.Validation.CarrierIdCompanyValidationRule>();
            services.AddScoped<IValidationRule<ShippingDto, Shipping>, Application.BusinessModels.Shippings.Validation.CarrierIdValidationRule>();
            services.AddScoped<IValidationRule<ShippingDto, Shipping>, PoolingRoutesValidationRule>();
            services.AddScoped<IValidationRule<ShippingDto, Shipping>, RoutesValidationRule>();
            services.AddScoped<IValidationRule<ShippingDto, Shipping>, Application.BusinessModels.Shippings.Validation.TarifficationTypeReadonlyRule>();
            services.AddScoped<IValidationRule<ShippingDto, Shipping>, Application.BusinessModels.Shippings.Validation.VehicleTypeIdCompanyValidationRule>();

            services.AddScoped<IAppAction<Shipping>, SendShippingToTk>();
            services.AddScoped<IAppAction<Shipping>, ConfirmShipping>();
            services.AddScoped<IAppAction<Shipping>, RejectRequestShipping>();
            services.AddScoped<IAppAction<Shipping>, CancelRequestShipping>();
            services.AddScoped<IAppAction<Shipping>, CompleteShipping>();
            services.AddScoped<IAppAction<Shipping>, CancelShipping>();
            services.AddScoped<IAppAction<Shipping>, ProblemShipping>();
            services.AddScoped<IAppAction<Shipping>, BillingShipping>();
            services.AddScoped<IAppAction<Shipping>, ArchiveShipping>();
            services.AddScoped<IAppAction<Shipping>, RollbackShipping>();
            services.AddScoped<IAppAction<Shipping>, Application.BusinessModels.Shippings.Actions.CancelPoolingReservation>();
            services.AddScoped<IAppAction<Shipping>, Application.BusinessModels.Shippings.Actions.SendToPooling>();

            services.AddScoped<ITrigger<Shipping>, CalcShippingDeliveryCost>();
            services.AddScoped<ITrigger<Shipping>, CalcShippingTotalCosts>();
            services.AddScoped<ITrigger<Shipping>, SendChangesToPooling>();
            services.AddScoped<ITrigger<Shipping>, Application.BusinessModels.Shippings.Triggers.SendUpdateShippingNotification>();
            services.AddScoped<ITrigger<Shipping>, Application.BusinessModels.Shippings.Triggers.SyncBodyType>();
            services.AddScoped<ITrigger<Shipping>, Application.BusinessModels.Shippings.Triggers.SyncShippingOrderFields>();
            services.AddScoped<ITrigger<Shipping>, Application.BusinessModels.Shippings.Triggers.SyncVehicleType>();
            services.AddScoped<ITrigger<Shipping>, UpdateBasicDeliveryCost>();
            services.AddScoped<ITrigger<Shipping>, Application.BusinessModels.Shippings.Triggers.UpdateManualFieldFlags>();
            services.AddScoped<ITrigger<Shipping>, UpdateOptimalVehicleType>();
            services.AddScoped<ITrigger<Shipping>, UpdateTotalDeliveryCostWithoutVAT>();

            services.AddScoped<IValidationTrigger<Shipping>, Application.BusinessModels.Shippings.Triggers.ValidateClearBacklightFlags>();
            services.AddScoped<IValidationTrigger<Shipping>, Application.BusinessModels.Shippings.Triggers.ValidateSendChangesToPooling>();

            services.AddScoped<IBacklight<Shipping>, Application.BusinessModels.Shippings.Backlights.CarrierRequestSentBacklight>();
        }

        private static void AddDictionariesBusinessModels(IServiceCollection services)
        {
            services.AddScoped<ITrigger<Article>, SyncArticleFields>();

            services.AddScoped<IValidationRule<ShippingWarehouseDto, ShippingWarehouse>, Application.BusinessModels.ShippingWarehouses.Validation.AddressValidationRule>();

            services.AddScoped<ITrigger<ShippingWarehouse>, SyncShippingWarehouseName>();
            services.AddScoped<ITrigger<ShippingWarehouse>, SyncShippingWarehouseFields>();
            services.AddScoped<ITrigger<ShippingWarehouse>, Application.BusinessModels.ShippingWarehouses.Triggers.UpdateShippingAddress>();

            services.AddScoped<ITrigger<Tariff>, CalcTariffDeliveryCost>();
            services.AddScoped<ITrigger<Tariff>, DeactivateOverlappedTariffs>();

            services.AddScoped<IValidationRule<WarehouseDto, Warehouse>, Application.BusinessModels.Warehouses.Validation.AddressValidationRule>();

            services.AddScoped<ITrigger<Warehouse>, SyncWarehouseFields>();
            services.AddScoped<ITrigger<Warehouse>, Application.BusinessModels.Warehouses.Triggers.UpdateDeliveryAddress>();
        }
    }
}