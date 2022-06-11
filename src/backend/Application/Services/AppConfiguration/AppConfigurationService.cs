using Application.Services.Articles;
using Application.Services.AutogroupingSettings;
using Application.Services.BodyTypes;
using Application.Services.Companies;
using Application.Services.DocumentTypes;
using Application.Services.Drivers;
using Application.Services.FixedDirections;
using Application.Services.Leadtime;
using Application.Services.Orders;
using Application.Services.PickingTypes;
using Application.Services.Shippings;
using Application.Services.ShippingSchedules;
using Application.Services.ShippingWarehouses;
using Application.Services.Tariffs;
using Application.Services.Tonnages;
using Application.Services.TransportCompanies;
using Application.Services.VehicleTypes;
using Application.Services.Warehouses;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Services.Articles;
using Domain.Services.AutogroupingSettings;
using Domain.Services.BodyTypes;
using Domain.Services.Companies;
using Domain.Services.DocumentTypes;
using Domain.Services.Drivers;
using Domain.Services.FieldProperties;
using Domain.Services.FixedDirections;
using Domain.Services.Identity;
using Domain.Services.Leadtime;
using Domain.Services.Orders;
using Domain.Services.PickingTypes;
using Domain.Services.Shippings;
using Domain.Services.ShippingSchedules;
using Domain.Services.ShippingWarehouses;
using Domain.Services.Tariffs;
using Domain.Services.Tonnages;
using Domain.Services.TransportCompanies;
using Domain.Services.VehicleTypes;
using Domain.Services.Warehouses;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DictionaryConfigMethod = System.Func<Domain.Persistables.User,
                                           System.Collections.Generic.List<Domain.Persistables.FieldPropertyItem>,
                                           System.Collections.Generic.List<Domain.Persistables.FieldPropertyItemVisibility>,
                                           Domain.Services.AppConfiguration.UserConfigurationDictionaryItem>;

namespace Application.Services.AppConfiguration
{
    public class AppConfigurationService : AppConfigurationServiceBase, IAppConfigurationService
    {
        private readonly IIdentityService _identityService;
        private readonly IUserProvider _userProvider;
        private readonly ICommonDataService _dataService;
        private readonly IFieldDispatcherService _fieldDispatcherService;
        private readonly IFieldPropertiesService _fieldPropertiesService;

        private readonly List<DictionaryConfig> _dictConfigurations = new List<DictionaryConfig>();

        public AppConfigurationService(
            IIdentityService identityService,
            IUserProvider userProvider,
            ICommonDataService dataService,
            IFieldDispatcherService fieldDispatcherService,
            IFieldPropertiesService fieldPropertiesService)
        {
            _identityService = identityService;
            _userProvider = userProvider;
            _dataService = dataService;
            _fieldDispatcherService = fieldDispatcherService;
            _fieldPropertiesService = fieldPropertiesService;

            InitDictionaryConfigurations();
        }

        public AppConfigurationDto GetConfiguration()
        {
            var userId = _userProvider.GetCurrentUserId();
            var user = _dataService
                .GetDbSet<User>()
                .Include(i => i.Role)
                .First(i => i.Id == userId);

            var matrixItems = _fieldPropertiesService.LoadMatrixItems(user.CompanyId, user.RoleId, null);
            var visibilities = _fieldPropertiesService.LoadVisibilities(user.CompanyId, user.RoleId, null);

            return new AppConfigurationDto
            {
                EditUsers = _identityService.HasPermissions(user, RolePermissions.UsersEdit),
                EditRoles = _identityService.HasPermissions(user, RolePermissions.RolesEdit),
                EditFieldProperties = _identityService.HasPermissions(user, RolePermissions.FieldsSettings),
                EditAutogroupingSettings = _identityService.HasPermissions(user, RolePermissions.AutogroupingSettingsEdit),
                ImportShippingVehicle = _identityService.HasPermissions(user, RolePermissions.ImportShippingVehicleDetails),
                ImportOrders = _identityService.HasPermissions(user, RolePermissions.ImportOrders),
                AutogroupingOrders = _identityService.HasPermissions(user, RolePermissions.AutogroupingOrders),
                ViewOperationalReport = _identityService.HasPermissions(user, RolePermissions.ViewOperationalReport),
                InvoiceImport = _identityService.HasPermissions(user, RolePermissions.InvoiceImport),
                ViewRegistryReport = _identityService.HasPermissions(user, RolePermissions.ViewRegistryReport),
                PoolingWarehousesImport = _identityService.HasPermissions(user, RolePermissions.PoolingWarehousesImport),
                Grids = GetGridsConfiguration(user, matrixItems, visibilities),
                Dictionaries = GetDictionariesConfiguration(user, matrixItems, visibilities)
            };
        }

        public IEnumerable<UserConfigurationGridItem> GetGridsConfiguration(User user, List<FieldPropertyItem> matrixItems, List<FieldPropertyItemVisibility> visibilities)
        {
            var userCompanyId = _userProvider.GetCurrentUser()?.CompanyId;
            var grids = new List<UserConfigurationGridItem>();

            if (_identityService.HasPermissions(user, RolePermissions.OrdersView))
            {
                var columns = ExtractColumnsFromDto<OrderDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                grids.Add(new UserConfigurationGridItem
                {
                    Name = GetName<OrdersService>(),
                    CanCreateByForm = userCompanyId != null && _identityService.HasPermissions(user, RolePermissions.OrdersCreate),
                    CanViewAdditionSummary = true,
                    CanExportToExcel = true,
                    CanImportFromExcel = false,
                    Columns = columns
                });
            }

            if (_identityService.HasPermissions(user, RolePermissions.ShippingsView))
            {
                var columns = ExtractColumnsFromDto<ShippingDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                grids.Add(new UserConfigurationGridItem
                {
                    Name = GetName<ShippingsService>(),
                    CanCreateByForm = userCompanyId != null && _identityService.HasPermissions(user, RolePermissions.ShippingsCreate),
                    CanViewAdditionSummary = true,
                    CanExportToExcel = true,
                    CanImportFromExcel = false,
                    Columns = columns
                });
            }

            return grids;
        }

        public IEnumerable<UserConfigurationDictionaryItem> GetDictionariesConfiguration(User user, List<FieldPropertyItem> matrixItems, List<FieldPropertyItemVisibility> visibilities)
        {
            return _dictConfigurations
                .Select(i => i.ConfigMethod(user, matrixItems, visibilities))
                .Where(i => i != null)
                .ToList();
        }

        public UserConfigurationDictionaryItem GetDictionaryConfiguration(Type serviceType)
        {
            var userId = _userProvider.GetCurrentUserId();
            var user = _dataService
                .GetDbSet<User>()
                .Include(i => i.Role)
                .First(i => i.Id == userId);

            var matrixItems = _fieldPropertiesService.LoadMatrixItems(user.CompanyId, user.RoleId, null);
            var visibilities = _fieldPropertiesService.LoadVisibilities(user.CompanyId, user.RoleId, null);

            var config = _dictConfigurations.FirstOrDefault(i => i.ServiceType.Name == serviceType.Name);

            return config != null ? config.ConfigMethod(user, matrixItems, visibilities) : null;
        }

        private void InitDictionaryConfigurations()
        {
            AddConfig<CompaniesService>((user, matrixItems, visibilities) =>
                {
                    var isGlobalUser = user?.CompanyId == null;

                    var canEditCompanies = isGlobalUser && _identityService.HasPermissions(user, RolePermissions.CompaniesEdit);
                    if (canEditCompanies)
                    {
                        var columns = ExtractColumnsFromDto<CompanyDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                        return new UserConfigurationDictionaryItem
                        {
                            Name = GetName<CompaniesService>(),
                            CanCreateByForm = canEditCompanies,
                            CanExportToExcel = true,
                            CanImportFromExcel = canEditCompanies,
                            ShowOnHeader = false,
                            Columns = columns
                        };
                    }

                    return null;
                });

            AddConfig<TariffsService>((user, matrixItems, visibilities) =>
            {
                var canEditTariffs = _identityService.HasPermissions(user, RolePermissions.TariffsEdit);
                var canViewTariffs = _identityService.HasPermissions(user, RolePermissions.TariffsView);

                if (canViewTariffs || canEditTariffs)
                {
                    var columns = ExtractColumnsFromDto<TariffDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<TariffsService>(),
                        CanCreateByForm = canEditTariffs,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditTariffs,
                        CanDelete = true,
                        ShowOnHeader = true,
                        Columns = columns
                    };
                }

                return null;
            });

            AddConfig<WarehousesService>((user, matrixItems, visibilities) =>
            {
                var canEditWarehouses = _identityService.HasPermissions(user, RolePermissions.WarehousesEdit);

                if (canEditWarehouses)
                {
                    var columns = ExtractColumnsFromDto<WarehouseDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<WarehousesService>(),
                        CanCreateByForm = canEditWarehouses,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditWarehouses,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });

            AddConfig<ShippingWarehousesService>((user, matrixItems, visibilities) =>
            {
                var canEditShippingWarehouses = _identityService.HasPermissions(user, RolePermissions.ShippingWarehousesEdit);
                var canEditWarehouses = _identityService.HasPermissions(user, RolePermissions.WarehousesEdit);

                if (canEditShippingWarehouses)
                {
                    var columns = ExtractColumnsFromDto<ShippingWarehouseDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<ShippingWarehousesService>(),
                        CanCreateByForm = canEditWarehouses,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditWarehouses,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });

            AddConfig<ArticlesService>((user, matrixItems, visibilities) =>
            {
                var canEditArticles = _identityService.HasPermissions(user, RolePermissions.ArticlesEdit);

                if (canEditArticles)
                {
                    var columns = ExtractColumnsFromDto<ArticleDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<ArticlesService>(),
                        CanCreateByForm = canEditArticles,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditArticles,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });

            AddConfig<PickingTypesService>((user, matrixItems, visibilities) =>
            {
                var canEditPickingTypes = _identityService.HasPermissions(user, RolePermissions.PickingTypesEdit);

                if (canEditPickingTypes)
                {
                    var columns = ExtractColumnsFromDto<PickingTypeDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<PickingTypesService>(),
                        CanCreateByForm = canEditPickingTypes,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditPickingTypes,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });

            AddConfig<TransportCompaniesService>((user, matrixItems, visibilities) =>
            {
                var canEditTransportCompanies = _identityService.HasPermissions(user, RolePermissions.TransportCompaniesEdit);

                if (canEditTransportCompanies)
                {
                    var columns = ExtractColumnsFromDto<TransportCompanyDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<TransportCompaniesService>(),
                        CanCreateByForm = canEditTransportCompanies,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditTransportCompanies,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });

            AddConfig<VehicleTypesService>((user, matrixItems, visibilities) =>
            {
                var canEditVehicleTypes = _identityService.HasPermissions(user, RolePermissions.VehicleTypesEdit);

                if (canEditVehicleTypes)
                {
                    var columns = ExtractColumnsFromDto<VehicleTypeDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<VehicleTypesService>(),
                        CanCreateByForm = canEditVehicleTypes,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditVehicleTypes,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }
                return null;
            });

            AddConfig<BodyTypesService>((user, matrixItems, visibilities) =>
            {
                var canEditVehicleTypes = _identityService.HasPermissions(user, RolePermissions.VehicleTypesEdit);

                if (canEditVehicleTypes)
                {
                    var bodyTypeColumns = ExtractColumnsFromDto<BodyTypeDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<BodyTypesService>(),
                        CanCreateByForm = canEditVehicleTypes,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditVehicleTypes,
                        ShowOnHeader = false,
                        Columns = bodyTypeColumns
                    };
                }

                return null;
            });

            AddConfig<TonnagesService>((user, matrixItems, visibilities) =>
            {
                var canEditVehicleTypes = _identityService.HasPermissions(user, RolePermissions.VehicleTypesEdit);

                if (canEditVehicleTypes)
                {
                    var tonnageColumns = ExtractColumnsFromDto<TonnageDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<TonnagesService>(),
                        CanCreateByForm = canEditVehicleTypes,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditVehicleTypes,
                        ShowOnHeader = false,
                        Columns = tonnageColumns
                    };
                }

                return null;
            });

            AddConfig<DocumentTypesService>((user, matrixItems, visibilities) =>
            {
                var canEditDocumentTypes = _identityService.HasPermissions(user, RolePermissions.DocumentTypesEdit);

                if (canEditDocumentTypes)
                {
                    var columns = ExtractColumnsFromDto<DocumentTypeDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<DocumentTypesService>(),
                        CanCreateByForm = canEditDocumentTypes,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditDocumentTypes,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });

            AddConfig<AutogroupingSettingsService>((user, matrixItems, visibilities) =>
            {
                var canEditAutogroupingSettings = _identityService.HasPermissions(user, RolePermissions.AutogroupingSettingsEdit);
                if (canEditAutogroupingSettings)
                {
                    var columns = ExtractColumnsFromDto<AutogroupingSettingDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<AutogroupingSettingsService>(),
                        CanCreateByForm = canEditAutogroupingSettings,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditAutogroupingSettings,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });

            AddConfig<LeadtimeService>((user, matrixItems, visibilities) =>
            {
                var canEditLeadtime = _identityService.HasPermissions(user, RolePermissions.LeadtimeEdit);
                if (canEditLeadtime)
                {
                    var columns = ExtractColumnsFromDto<LeadtimeDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<LeadtimeService>(),
                        CanCreateByForm = canEditLeadtime,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditLeadtime,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });
            AddConfig<DriversService>((user, matrixItems, visibilities) =>
            {
                var canEditDrivers = _identityService.HasPermissions(user, RolePermissions.EditDrivers);
                if (canEditDrivers)
                {
                    var columns = ExtractColumnsFromDto<DriverDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<DriversService>(),
                        CanCreateByForm = canEditDrivers,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditDrivers,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });
            AddConfig<ShippingSchedulesService>((user, matrixItems, visibilities) =>
            {
                var canEditShippingSchedule = _identityService.HasPermissions(user, RolePermissions.EditShippingSchedule);
                if (canEditShippingSchedule)
                {
                    var columns = ExtractColumnsFromDto<ShippingScheduleDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<ShippingSchedulesService>(),
                        CanCreateByForm = canEditShippingSchedule,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditShippingSchedule,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });

            AddConfig<FixedDirectionsService>((user, matrixItems, visibilities) =>
            {
                var canEditFixedDirections = _identityService.HasPermissions(user, RolePermissions.FixedDirectionsEdit);
                if (canEditFixedDirections)
                {
                    var columns = ExtractColumnsFromDto<FixedDirectionDto>(user.CompanyId, user.RoleId, matrixItems, visibilities);
                    return new UserConfigurationDictionaryItem
                    {
                        Name = GetName<FixedDirectionsService>(),
                        CanCreateByForm = canEditFixedDirections,
                        CanExportToExcel = true,
                        CanImportFromExcel = canEditFixedDirections,
                        ShowOnHeader = false,
                        Columns = columns
                    };
                }

                return null;
            });
        }

        private void AddConfig<TService>(DictionaryConfigMethod method)
        {
            _dictConfigurations.Add(new DictionaryConfig
            {
                ServiceType = typeof(TService),
                ConfigMethod = method
            });
        }

        private FieldPropertiesForEntityType? GetFieldPropertyForEntity<TDto>()
        {
            if (typeof(TDto) == typeof(OrderDto))
            {
                return FieldPropertiesForEntityType.Orders;
            }
            else if (typeof(TDto) == typeof(ShippingDto))
            {
                return FieldPropertiesForEntityType.Shippings;
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<UserConfigurationGridColumn> ExtractColumnsFromDto<TDto>(
            Guid? companyId,
            Guid? roleId,
            List<FieldPropertyItem> matrixItems,
            List<FieldPropertyItemVisibility> visibilities)
        {
            var fields = _fieldDispatcherService.GetDtoFields<TDto>();

            var forEntity = GetFieldPropertyForEntity<TDto>();
            if (forEntity.HasValue)
            {
                var availableFieldNames = _fieldPropertiesService.GetAvailableFields(forEntity.Value, companyId, roleId, null, matrixItems, visibilities);
                fields = fields.Where(x => availableFieldNames.Any(y => string.Compare(x.Name, y, true) == 0));
            }

            var result = new List<UserConfigurationGridColumn>();
            foreach (var field in fields.OrderBy(f => f.OrderNumber))
            {
                if (!string.IsNullOrEmpty(field.ReferenceSource) || field.Dependencies?.Length > 0)
                {
                    result.Add(new UserConfigurationGridColumnWhitchSource(field));
                }
                else
                {
                    result.Add(new UserConfigurationGridColumn(field));
                }
            }

            return result;
        }
    }
}