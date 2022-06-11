using Application.Extensions;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Shippings;
using Application.Shared.Triggers;
using AutoMapper;
using DAL.Extensions;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.AppConfiguration;
using Domain.Services.Autogrouping;
using Domain.Services.FieldProperties;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Application.Services.Autogrouping
{
    public class AutogroupingService : IAutogroupingService
    {
        private readonly ICommonDataService _dataService;
        private readonly IShippingActionService _shippingActionService;
        private readonly ISendShippingService _sendShippingService;
        private readonly IShippingCalculationService _shippingCalculationService;
        private readonly IUserProvider _userProvider;
        private readonly ITriggersService _triggersService;
        private readonly IFieldDispatcherService _fieldDispatcherService;
        private readonly IGroupingOrdersService _groupingOrdersService;
        private readonly IMapper _mapper;

        private const string _typesSettingsKey = "AG_TYPES";

        public AutogroupingService(
            ICommonDataService dataService,
            IShippingActionService shippingActionService,
            ISendShippingService sendShippingService,
            IShippingCalculationService shippingCalculationService,
            IUserProvider userProvider,
            ITriggersService triggersService,
            IFieldDispatcherService fieldDispatcherService,
            IGroupingOrdersService groupingOrdersService)
        {
            _dataService = dataService;
            _shippingActionService = shippingActionService;
            _sendShippingService = sendShippingService;
            _shippingCalculationService = shippingCalculationService;
            _userProvider = userProvider;
            _triggersService = triggersService;
            _fieldDispatcherService = fieldDispatcherService;
            _groupingOrdersService = groupingOrdersService;
            _mapper = ConfigureMapper().CreateMapper();
        }

        public RunResponse RunGrouping(RunRequest request)
        {
            var runId = Guid.NewGuid();
            var runAt = DateTime.Now;

            var types = request?.AutogroupingTypes?
                                .Select(x => x.Value.ToEnum<AutogroupingType>())
                                .Where(x => x != null)
                                .Select(x => x.Value)
                                .ToList();
            SaveSelectedTypes(types);

            var orderIds = request?.Ids?.Select(x => x.ToGuid()).ToList() ?? new List<Guid?>();
            var orders = _dataService.GetDbSet<Order>()
                                     .Include(x => x.ShippingWarehouse)
                                     .Include(x => x.DeliveryWarehouse)
                                     .Where(x => orderIds.Contains(x.Id))
                                     .ToList();

            var result = _groupingOrdersService.GroupOrders(orders, runId, types);
            if (result?.Orders != null && result?.Shippings != null)
            {
                _dataService.GetDbSet<AutogroupingOrder>().AddRange(result.Orders);
                _dataService.GetDbSet<AutogroupingShipping>().AddRange(result.Shippings);
                _dataService.GetDbSet<AutogroupingCost>().AddRange(result.Costs);

                _dataService.SaveChanges();
            }

            return new RunResponse
            {
                RunId = runId.FormatGuid()
            };
        }

        public void ChangeCarrier(Guid runId, ChangeCarrierRequest request)
        {
            var shippingId = request?.ShippingId.ToGuid();
            var shipping = _dataService.GetDbSet<AutogroupingShipping>()
                                       .Where(x => x.RunId == runId && x.Id == shippingId)
                                       .FirstOrDefault();
            if (shipping == null)
            {
                throw new NotFoundException();
            }

            var carrierId = request?.CarrierId.ToGuid();
            var newCost = _dataService.GetDbSet<AutogroupingCost>()
                                      .Where(x => x.AutogroupingShippingId == shippingId
                                                && x.CarrierId == carrierId
                                                && x.AutogroupingType == shipping.AutogroupingType)
                                      .FirstOrDefault();
            if (newCost == null)
            {
                throw new NotFoundException();
            }

            shipping.BestCost = newCost.Value;
            shipping.CarrierId = carrierId;
            switch (shipping.AutogroupingType)
            {
                case AutogroupingType.FtlDirect:
                    shipping.FtlDirectCost = newCost.Value;
                    break;

                case AutogroupingType.FtlRoute:
                    shipping.FtlRouteCost = newCost.Value;
                    break;

                case AutogroupingType.Ltl:
                    shipping.LtlCost = newCost.Value;
                    break;

                case AutogroupingType.Milkrun:
                    shipping.MilkrunCost = newCost.Value;
                    break;

                case AutogroupingType.Pooling:
                    shipping.PoolingCost = newCost.Value;
                    break;
            }

            _dataService.SaveChanges();
        }

        public ValidateResult MoveOrders(Guid runId, MoveOrderRequest request)
        {
            var result = ValidateMoveOrdersRequest(request);
            if (result.IsError)
            {
                return result;
            }

            var targetShippingId = request?.NewShippingId.ToGuid();
            var targetShipping = _dataService.GetDbSet<AutogroupingShipping>()
                                             .Where(x => x.RunId == runId && x.Id == targetShippingId)
                                             .FirstOrDefault();
            if (targetShipping == null)
            {
                throw new NotFoundException();
            }

            var orderIds = request?.OrderIds?.Select(x => x.ToGuid())?.ToList();
            var orders = _dataService.GetDbSet<AutogroupingOrder>()
                                     .Include(x => x.Order)
                                     .Include(x => x.Order.ShippingWarehouse)
                                     .Include(x => x.Order.DeliveryWarehouse)
                                     .Where(x => x.RunId == runId && orderIds.Contains(x.Id))
                                     .ToList();
            var loadedOrderIds = orders.Select(x => x.Id).ToHashSet();
            if (orderIds.Any(x => !loadedOrderIds.Contains(x.Value)))
            {
                throw new NotFoundException();
            }

            var types = GetSelectedTypes();
            result = _groupingOrdersService.MoveOrders(orders, targetShipping, types);

            return result;
        }

        public OperationDetailedResult Apply(Guid runId, List<Guid> rowIds)
        {
            var groupShippings = LoadGroupShippings(runId, rowIds);
            var groupOrders = LoadGroupShippingOrders(runId, rowIds);

            if (!groupShippings.Any())
            {
                var lang = _userProvider.GetCurrentUser()?.Language;
                var emptyResult = new OperationDetailedResult
                {
                    Error = "AutogroupingEmptyOrdersError".Translate(lang),
                    IsError = true
                };
                return emptyResult;
            }

            List<string> newShippingOrders;
            List<string> newShippingNumbers;
            CreateShippings(groupShippings, groupOrders, out newShippingOrders, out newShippingNumbers, out _);

            var result = FillApplyResults(newShippingOrders, newShippingNumbers, null, null);

            ApplyDbChanges();

            return result;
        }

        public OperationDetailedResult ApplyAndSend(Guid runId, List<Guid> rowIds)
        {
            var user = _userProvider.GetCurrentUser();
            var lang = user?.Language;

            var groupShippings = LoadGroupShippings(runId, rowIds);
            var groupOrders = LoadGroupShippingOrders(runId, rowIds);

            if (!groupShippings.Any())
            {
                var emptyResult = new OperationDetailedResult
                {
                    Error = "AutogroupingEmptyOrdersError".Translate(lang),
                    IsError = true
                };
                return emptyResult;
            }

            List<string> newShippingOrders;
            List<string> newShippingNumbers;
            Dictionary<Guid, List<Order>> shippingOrdersDict;
            var shippings = CreateShippings(groupShippings, groupOrders, out newShippingOrders, out newShippingNumbers, out shippingOrdersDict);

            var sentShippingNumbers = new List<string>();
            var errorMessages = new List<string>();
            foreach (var shipping in shippings)
            {
                shippingOrdersDict.TryGetValue(shipping.Id, out List<Order> shippingOrders);

                if (shipping.TarifficationType == TarifficationType.Pooling
                    || shipping.TarifficationType == TarifficationType.Milkrun)
                {
                    var sendResult = _sendShippingService.SendShippingToPooling(user, shipping, shippingOrders);
                    if (sendResult.IsError)
                    {
                        errorMessages.Add("AutogroupingErrorMessage".Translate(lang, shipping.ShippingNumber, sendResult.Message));
                    }
                    else
                    {
                        sentShippingNumbers.Add(shipping.ShippingNumber);
                    }
                }
                else
                {
                    _sendShippingService.SendShippingToTk(shipping, shippingOrders);
                    sentShippingNumbers.Add(shipping.ShippingNumber);
                }
            }

            var result = FillApplyResults(newShippingOrders, newShippingNumbers, sentShippingNumbers, errorMessages);

            ApplyDbChanges();

            return result;
        }

        public SearchResult<AutogroupingShippingDto> Search(Guid runId, FilterFormDto<AutogroupingFilterDto> dto)
        {
            var dbSet = GetDbSet();

            var query = ApplySearchForm(dbSet, runId, dto);

            if (dto.Take == 0)
                dto.Take = 1000;

            var totalCount = query.Count();
            var entities = ApplySort(query, dto)
                .Skip(dto.Skip)
                .Take(dto.Take).ToList();

            var items = entities.Select(x => _mapper.Map<AutogroupingShippingDto>(x)).ToList();

            var shippingIds = entities.Select(x => x.Id).ToList();
            FillAlternativeCosts(items, shippingIds);

            var result = new SearchResult<AutogroupingShippingDto>
            {
                TotalCount = totalCount,
                Items = items
            };

            return result;
        }

        public IEnumerable<string> SearchIds(Guid runId, FilterFormDto<AutogroupingFilterDto> dto)
        {
            var dbSet = GetDbSet();
            var query = ApplySearchForm(dbSet, runId, dto);
            var ids = query.Select(e => e.Id).ToList();
            var result = ids.Select(x => x.FormatGuid()).ToList();
            return result;
        }

        public IEnumerable<LookUpDto> ForSelect(Guid runId, string fieldName, FilterFormDto<AutogroupingFilterDto> form)
        {
            foreach (var prop in form.Filter.GetType().GetProperties())
            {
                if (string.Equals(prop.Name, fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    prop.SetValue(form.Filter, null);
                }
            }

            var user = _userProvider.GetCurrentUser();

            var dbSet = GetDbSet();
            var query = ApplySearchForm(dbSet, runId, form);

            var propertyInfo = typeof(AutogroupingShipping).GetProperties()
                .FirstOrDefault(i => i.Name.ToLower() == fieldName.ToLower());
            var refType = GetReferenceType(propertyInfo);

            var fields = _fieldDispatcherService.GetDtoFields<AutogroupingShippingDto>();
            var field = fields.FirstOrDefault(i => i.Name.ToLower() == fieldName.ToLower());

            IEnumerable<LookUpDto> result;

            if (refType != null)
            {
                result = GetReferencedValues(query, refType, fieldName);
            }
            else if (field.FieldType == FieldType.State)
            {
                result = GetStateValues(query, propertyInfo);
            }
            else
            {
                result = GetSelectValues(query, propertyInfo, field.ShowRawReferenceValue);
            }

            if (field.EmptyValueOptions != EmptyValueOptions.NotAllowed)
            {
                var empty = new LookUpDto
                {
                    Name = "emptyValue".Translate(user.Language),
                    Value = LookUpDto.EmptyValue,
                    IsFilterOnly = field.EmptyValueOptions == EmptyValueOptions.FilterOnly
                };

                result = new[] { empty }.Union(result);
            }

            return result;
        }

        public AutogroupingSummaryDto GetSummary(Guid runId)
        {
            var ordersCount = _dataService.GetDbSet<AutogroupingOrder>().Where(x => x.RunId == runId).Count();
            var shippings = _dataService.GetDbSet<AutogroupingShipping>().Where(x => x.RunId == runId).ToList();
            var result = new AutogroupingSummaryDto
            {
                OrdersCount = ordersCount,
                ShippingsCount = shippings.Count,
                PalletsCount = (int)Math.Ceiling(shippings.Sum(x => x.PalletsCount ?? 0M)),
                TotalAmount = shippings.Sum(o => o.BestCost ?? 0M)
            };
            return result;
        }

        public Stream ExportToExcel(Guid runId, ExportExcelFormDto<AutogroupingFilterDto> dto)
        {
            var excel = new ExcelPackage();
            var shippingSheet = excel.Workbook.Worksheets.Add("Перевозки");
            var orderSheet = excel.Workbook.Worksheets.Add("Накладные");

            var types = GetSelectedTypes();

            var dbSet = GetDbSet();
            var query = ApplySearchForm(dbSet, runId, dto);
            var entities = ApplySort(query, dto).ToList();

            FillShippingSheet(shippingSheet, entities, types);
            FillOrderSheet(orderSheet, entities);

            return new MemoryStream(excel.GetAsByteArray());
        }

        public AutogroupingTypesDto GetAutogroupingTypes()
        {
            var lang = _userProvider.GetCurrentUser()?.Language;
            var types = GetSelectedTypes();

            var all = new List<LookUpDto>();
            var selected = new List<LookUpDto>();

            foreach (var type in Enum.GetValues(typeof(AutogroupingType)).Cast<AutogroupingType>())
            {
                var value = type.FormatEnum();
                var name = value.Translate(lang);
                var displayName = "AutogroupingTypeSettingsOption".Translate(lang, name);
                var entry = new LookUpDto(value, displayName);

                all.Add(entry);
                if (types != null && types.Contains(type))
                {
                    selected.Add(entry);
                }
            }

            return new AutogroupingTypesDto
            {
                All = all,
                Selected = selected
            };
        }

        private ValidateResult ValidateMoveOrdersRequest(MoveOrderRequest request)
        {
            var errors = new List<string>();
            var lang = _userProvider.GetCurrentUser()?.Language;

            var targetShippingId = request?.NewShippingId.ToGuid();
            if (targetShippingId == null)
            {
                errors.Add("Autogrouping.MoveOrders.EmptyTargetShippingId".Translate(lang));
            }

            var orderIds = request?.OrderIds?.Select(x => x.ToGuid())?.ToList();
            if (orderIds == null || orderIds.Any(x => x == null))
            {
                errors.Add("Autogrouping.MoveOrders.EmptyTargetOrderIds".Translate(lang));
            }

            return new ValidateResult(string.Join(' ', errors), errors.Count > 0);
        }

        private void FillAlternativeCosts(List<AutogroupingShippingDto> items, List<Guid> shippingIds)
        {
            var alternativeCosts = _dataService.GetDbSet<AutogroupingCost>()
                                               .Include(x => x.Carrier)
                                               .Where(x => shippingIds.Contains(x.AutogroupingShippingId))
                                               .GroupBy(x => x.AutogroupingShippingId)
                                               .ToDictionary(x => x.Key.FormatGuid(), x => x.ToList());

            foreach (var item in items)
            {
                var autogroupingType = item.AutogroupingType?.Value.ToEnum<AutogroupingType>();
                if (item.CarrierId != null && alternativeCosts.TryGetValue(item.Id, out List<AutogroupingCost> costs))
                {
                    item.CarrierId.AlternativeCosts = new List<AlternativeCostDto>();
                    foreach (var cost in costs.Where(x => x.AutogroupingType == autogroupingType)
                                              .OrderBy(x => x.Value))
                    {
                        item.CarrierId.AlternativeCosts.Add(new AlternativeCostDto
                        {
                            CarrierId = cost.CarrierId.FormatGuid(),
                            CarrierName = cost.Carrier?.ToString(),
                            Cost = cost.Value
                        });
                    }
                }
            }
        }

        private void FillShippingSheet(ExcelWorksheet sheet, List<AutogroupingShipping> entities, List<AutogroupingType> types)
        {
            var dtos = entities.Select(x => _mapper.Map<AutogroupingShippingDto>(x));

            var lang = _userProvider.GetCurrentUser()?.Language;
            var excelMapper = new ExcelMapper<AutogroupingShippingDto>(_dataService, _userProvider, _fieldDispatcherService)
                                .MapColumn(x => x.CarrierId, new DictionaryReferenceExcelColumn<TransportCompany>(_dataService, _userProvider, x => x.Title))
                                .MapColumn(x => x.VehicleTypeId, new DictionaryReferenceExcelColumn<VehicleType>(_dataService, _userProvider, x => x.Name))
                                .MapColumn(x => x.AutogroupingType, new EnumExcelColumn<AutogroupingType>(lang));

            foreach (var column in excelMapper.Columns.ToArray())
            {
                if (!IsFieldVisible(column.Property.Name, types))
                {
                    excelMapper.RemoveColumn(column);
                }
            }

            var user = _userProvider.GetCurrentUser();
            excelMapper.FillSheet(sheet, dtos, user.Language);
        }

        private void FillOrderSheet(ExcelWorksheet sheet, List<AutogroupingShipping> shippings)
        {
            var shippingIds = shippings.Select(x => x.Id).ToList();
            var entities = _dataService.GetDbSet<AutogroupingOrder>()
                                     .Include(x => x.AutogroupingShipping)
                                     .Include(x => x.ShippingWarehouse)
                                     .Include(x => x.DeliveryWarehouse)
                                     .Include(x => x.BodyType)
                                     .Include(x => x.VehicleType)
                                     .Where(x => x.AutogroupingShippingId != null && shippingIds.Contains(x.AutogroupingShippingId.Value))
                                     .OrderBy(x => x.AutogroupingShipping.ShippingNumber)
                                     .ThenBy(x => x.ShippingDate)
                                     .ThenBy(x => x.DeliveryDate)
                                     .ThenBy(x => x.CreatedAt)
                                     .ToList();

            var dtos = entities.Select(x => _mapper.Map<AutogroupingOrderExportDto>(x));

            var lang = _userProvider.GetCurrentUser()?.Language;
            var excelMapper = new ExcelMapper<AutogroupingOrderExportDto>(_dataService, _userProvider, _fieldDispatcherService)
                                .MapColumn(x => x.ShippingWarehouseId, new DictionaryReferenceExcelColumn<ShippingWarehouse>(_dataService, _userProvider, x => x.WarehouseName))
                                .MapColumn(x => x.DeliveryWarehouseId, new DictionaryReferenceExcelColumn<Warehouse>(_dataService, _userProvider, x => x.WarehouseName))
                                .MapColumn(x => x.VehicleTypeId, new DictionaryReferenceExcelColumn<VehicleType>(_dataService, _userProvider, x => x.Name))
                                .MapColumn(x => x.BodyTypeId, new DictionaryReferenceExcelColumn<BodyType>(_dataService, _userProvider, x => x.Name));

            var user = _userProvider.GetCurrentUser();
            excelMapper.FillSheet(sheet, dtos, user.Language);
        }

        public UserConfigurationGridItem GetPreviewConfiguration()
        {
            var types = GetSelectedTypes();

            var columns = new List<UserConfigurationGridColumn>();
            var fields = _fieldDispatcherService.GetDtoFields<AutogroupingShippingDto>();

            foreach (var field in fields.OrderBy(f => f.OrderNumber))
            {
                if (!IsFieldVisible(field.Name, types))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(field.ReferenceSource))
                {
                    columns.Add(new UserConfigurationGridColumn(field));
                }
                else
                {
                    columns.Add(new UserConfigurationGridColumnWhitchSource(field));
                }
            }

            return new UserConfigurationGridItem
            {
                Columns = columns
            };
        }

        private Dictionary<Guid, List<AutogroupingOrder>> LoadGroupShippingOrders(Guid runId, List<Guid> rowIds)
        {
            return _dataService.GetDbSet<AutogroupingOrder>()
                                                      .Where(x => x.RunId == runId
                                                                && x.AutogroupingShippingId != null
                                                                && rowIds.Contains(x.AutogroupingShippingId.Value))
                                                      .ToList()
                                                      .GroupBy(x => x.AutogroupingShippingId.Value)
                                                      .ToDictionary(x => x.Key, x => x.ToList());
        }

        private List<AutogroupingShipping> LoadGroupShippings(Guid runId, List<Guid> rowIds)
        {
            return _dataService.GetDbSet<AutogroupingShipping>()
                                             .Where(x => x.RunId == runId && rowIds.Contains(x.Id))
                                             .ToList();
        }

        private List<Shipping> CreateShippings(
            List<AutogroupingShipping> groupShippings,
            Dictionary<Guid, List<AutogroupingOrder>> groupOrders,
            out List<string> newShippingOrders,
            out List<string> newShippingNumbers,
            out Dictionary<Guid, List<Order>> shippingOrdersDict)
        {
            var shippings = new List<Shipping>();
            newShippingOrders = new List<string>();
            newShippingNumbers = new List<string>();
            shippingOrdersDict = new Dictionary<Guid, List<Order>>();

            var orderIds = groupOrders.SelectMany(x => x.Value.Select(y => y.OrderId)).ToList();
            var orders = _dataService.GetDbSet<Order>().Where(x => orderIds.Contains(x.Id)).ToDictionary(x => x.Id);

            var shippingNumbers = groupShippings.Select(x => x.ShippingNumber)
                                                .Where(x => !string.IsNullOrEmpty(x))
                                                .Distinct()
                                                .ToList();
            var reservedShippingNumbers = _dataService.GetDbSet<Shipping>()
                                                      .Where(x => shippingNumbers.Contains(x.ShippingNumber))
                                                      .Select(x => x.ShippingNumber)
                                                      .ToHashSet();

            foreach (var group in groupShippings)
            {
                if (group.AutogroupingType == null)
                {
                    continue;
                }

                List<AutogroupingOrder> groupShippingOrders;
                if (!groupOrders.TryGetValue(group.Id, out groupShippingOrders))
                {
                    continue;
                }

                var shippingOrders = new List<Order>();
                foreach (var groupOrder in groupShippingOrders)
                {
                    if (orders.TryGetValue(groupOrder.OrderId, out Order order))
                    {
                        order.BodyTypeId = groupOrder.BodyTypeId;
                        order.VehicleTypeId = groupOrder.VehicleTypeId;
                        shippingOrders.Add(order);
                    }
                }

                if (!shippingOrders.Any())
                {
                    continue;
                }

                var groupOrderNumbers = shippingOrders.Select(x => x.OrderNumber).ToList();

                if (!string.IsNullOrEmpty(group.ShippingNumber) && reservedShippingNumbers.Contains(group.ShippingNumber))
                {
                    group.ShippingNumber = ShippingNumberProvider.GetNextShippingNumber();
                }

                var shipping = _shippingActionService.UnionOrders(shippingOrders, group.ShippingNumber);

                shipping.CarrierId = group.CarrierId;
                shipping.TarifficationType = group.TarifficationType;
                foreach (var groupOrder in groupShippingOrders)
                {
                    if (orders.TryGetValue(groupOrder.OrderId, out Order order))
                    {
                        order.DeliveryCost = group.BestCost;
                        order.CarrierId = group.CarrierId;
                        order.TarifficationType = group.TarifficationType;
                    }
                }
                _shippingCalculationService.RecalculateDeliveryCosts(shipping, shippingOrders);

                shippingOrdersDict[shipping.Id] = shippingOrders;

                newShippingOrders.AddRange(groupOrderNumbers);
                newShippingNumbers.Add(shipping.ShippingNumber);
                shippings.Add(shipping);
            }

            return shippings;
        }

        private OperationDetailedResult FillApplyResults(
            List<string> newShippingOrders, List<string> newShippingNumbers,
            List<string> sentShippingNumbers, List<string> errorMessages)
        {
            var result = new OperationDetailedResult();
            var lang = _userProvider.GetCurrentUser()?.Language;

            int totalCount = newShippingOrders.Count;
            result.Message = "AutogroupingResultTitle".Translate(lang, totalCount);

            if (newShippingOrders != null && newShippingOrders.Any())
            {
                result.Entries.Add(new OperationDetailedResultItem
                {
                    Title = "AutogroupingNewShippingOrdersTitle".Translate(lang, newShippingOrders.Count, totalCount),
                    Messages = newShippingOrders.OrderBy(x => x).ToList(),
                    MessageColumns = 4,
                    IsError = false
                });
            }

            if (newShippingNumbers != null && newShippingNumbers.Any())
            {
                result.Entries.Add(new OperationDetailedResultItem
                {
                    Title = "AutogroupingNewShippingsTitle".Translate(lang, newShippingNumbers.Count),
                    Messages = newShippingNumbers.OrderBy(x => x).ToList(),
                    MessageColumns = 4,
                    IsError = false
                });
            }

            if (sentShippingNumbers != null && sentShippingNumbers.Any())
            {
                result.Entries.Add(new OperationDetailedResultItem
                {
                    Title = "AutogroupingSentShippingsTitle".Translate(lang, sentShippingNumbers.Count),
                    Messages = sentShippingNumbers.OrderBy(x => x).ToList(),
                    MessageColumns = 4,
                    IsError = false
                });
            }

            if (errorMessages != null && errorMessages.Any())
            {
                result.Entries.Add(new OperationDetailedResultItem
                {
                    Title = "AutogroupingErrorMessagesTitle".Translate(lang, errorMessages.Count),
                    Messages = errorMessages.OrderBy(x => x).ToList(),
                    MessageColumns = 1,
                    IsError = true
                });
            }

            return result;
        }

        private List<AutogroupingType> GetSelectedTypes()
        {
            var userId = _userProvider.GetCurrentUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException();
            }

            var settings = _dataService.GetDbSet<UserSetting>()
                                       .Where(x => x.UserId == userId && x.Key == _typesSettingsKey)
                                       .FirstOrDefault();

            List<AutogroupingType> result = null;
            if (!string.IsNullOrEmpty(settings?.Value))
            {
                result = JsonConvert.DeserializeObject<List<AutogroupingType>>(settings.Value);
            }

            if (result == null || !result.Any())
            {
                result = Enum.GetValues(typeof(AutogroupingType)).Cast<AutogroupingType>().ToList();
            }

            return result;
        }

        private void SaveSelectedTypes(List<AutogroupingType> types)
        {
            var userId = _userProvider.GetCurrentUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException();
            }

            var settings = _dataService.GetDbSet<UserSetting>()
                                       .Where(x => x.UserId == userId && x.Key == _typesSettingsKey)
                                       .FirstOrDefault();
            if (settings == null)
            {
                settings = new UserSetting
                {
                    Id = Guid.NewGuid(),
                    Key = _typesSettingsKey,
                    UserId = userId.Value
                };
                _dataService.GetDbSet<UserSetting>().Add(settings);
            }

            settings.Value = JsonConvert.SerializeObject(types);
        }

        private void ApplyDbChanges()
        {
            _triggersService.Execute(false);
            _dataService.SaveChanges();
        }

        private bool IsFieldVisible(string fieldName, List<AutogroupingType> types)
        {
            bool isEmptyAutogroupingTypeSettings = types == null || !types.Any();
            switch (fieldName)
            {
                case nameof(AutogroupingShippingDto.FtlRouteCost):
                    return isEmptyAutogroupingTypeSettings || types.Contains(AutogroupingType.FtlRoute);

                case nameof(AutogroupingShippingDto.FtlDirectCost):
                    return isEmptyAutogroupingTypeSettings || types.Contains(AutogroupingType.FtlDirect);

                case nameof(AutogroupingShippingDto.LtlCost):
                    return isEmptyAutogroupingTypeSettings || types.Contains(AutogroupingType.Ltl);

                case nameof(AutogroupingShippingDto.PoolingCost):
                    return isEmptyAutogroupingTypeSettings || types.Contains(AutogroupingType.Pooling);

                case nameof(AutogroupingShippingDto.MilkrunCost):
                    return isEmptyAutogroupingTypeSettings || types.Contains(AutogroupingType.Milkrun);

                default:
                    return true;
            }
        }

        private IQueryable<AutogroupingShipping> GetDbSet()
        {
            return _dataService.GetDbSet<AutogroupingShipping>()
                    .Include(x => x.Carrier)
                    .Include(x => x.VehicleType);
        }

        private IQueryable<AutogroupingShipping> ApplySort(IQueryable<AutogroupingShipping> query, FilterFormDto<AutogroupingFilterDto> form)
        {
            return query.OrderBy(form.Sort?.Name, form.Sort?.Desc == true)
                .DefaultOrderBy(i => i.CreatedAt, true);
        }

        private IQueryable<AutogroupingShipping> ApplySearchForm(IQueryable<AutogroupingShipping> query, Guid runId, FilterFormDto<AutogroupingFilterDto> searchForm)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(searchForm.Filter.AutogroupingType.ApplyEnumFilter<AutogroupingShipping, AutogroupingType>(i => i.AutogroupingType, ref parameters))
                         .WhereAnd(searchForm.Filter.CarrierId.ApplyOptionsFilter<AutogroupingShipping, Guid?>(i => i.CarrierId, ref parameters, i => i.ToGuid()))
                         .WhereAnd(searchForm.Filter.VehicleTypeId.ApplyOptionsFilter<AutogroupingShipping, Guid?>(i => i.VehicleTypeId, ref parameters, i => i.ToGuid()))
                         .WhereAnd(searchForm.Filter.DeliveryDate.ApplyDateRangeFilter<AutogroupingShipping>(i => i.DeliveryDate, ref parameters))
                         .WhereAnd(searchForm.Filter.FtlDirectCost.ApplyNumericFilter<AutogroupingShipping>(i => i.FtlDirectCost, ref parameters))
                         .WhereAnd(searchForm.Filter.FtlRouteCost.ApplyNumericFilter<AutogroupingShipping>(i => i.FtlRouteCost, ref parameters))
                         .WhereAnd(searchForm.Filter.LtlCost.ApplyNumericFilter<AutogroupingShipping>(i => i.LtlCost, ref parameters))
                         .WhereAnd(searchForm.Filter.MilkrunCost.ApplyNumericFilter<AutogroupingShipping>(i => i.MilkrunCost, ref parameters))
                         .WhereAnd(searchForm.Filter.OrdersCount.ApplyNumericFilter<AutogroupingShipping>(i => i.OrdersCount, ref parameters))
                         .WhereAnd(searchForm.Filter.PalletsCount.ApplyNumericFilter<AutogroupingShipping>(i => i.PalletsCount, ref parameters))
                         .WhereAnd(searchForm.Filter.PoolingCost.ApplyNumericFilter<AutogroupingShipping>(i => i.PoolingCost, ref parameters))
                         .WhereAnd(searchForm.Filter.Route.ApplyStringFilter<AutogroupingShipping>(i => i.Route, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingDate.ApplyDateRangeFilter<AutogroupingShipping>(i => i.ShippingDate, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingNumber.ApplyStringFilter<AutogroupingShipping>(i => i.ShippingNumber, ref parameters))
                         .WhereAnd($@"""{nameof(AutogroupingShipping.RunId)}"" = '{runId.ToString("D")}'");

            string sql = $@"SELECT * FROM ""AutogroupingShippings"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            // Apply Search
            return ApplySearch(query, searchForm?.Filter?.Search);
        }

        private IQueryable<AutogroupingShipping> ApplySearch(IQueryable<AutogroupingShipping> query, string search)
        {
            if (string.IsNullOrEmpty(search)) return query;

            search = search.ToLower().Trim();

            var isInt = int.TryParse(search, out int searchInt);

            decimal? searchDecimal = search.ToDecimal();
            var isDecimal = searchDecimal != null;
            decimal precision = 0.01M;

            var carriers = _dataService.GetDbSet<TransportCompany>().Where(i => i.Title.ToLower().Contains(search));

            var vehicleTypes = _dataService.GetDbSet<VehicleType>().Where(i => i.Name.ToLower().Contains(search));

            var autogroupingTypeNames = Enum.GetNames(typeof(AutogroupingType)).Select(i => i.ToLower());
            var autogroupingTypes = _dataService.GetDbSet<Translation>()
                .Where(i => autogroupingTypeNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<AutogroupingType>())
                .ToList();

            return query.Where(i =>
                   !string.IsNullOrEmpty(i.Route) && i.Route.ToLower().Contains(search)
                || !string.IsNullOrEmpty(i.ShippingNumber) && i.ShippingNumber.ToLower().Contains(search)

                || i.AutogroupingType != null && autogroupingTypes.Contains(i.AutogroupingType)
                || carriers.Any(p => p.Id == i.CarrierId)
                || vehicleTypes.Any(p => p.Id == i.VehicleTypeId)

                || isInt && i.OrdersCount == searchInt
                || isInt && i.PalletsCount == searchInt

                || isDecimal && i.FtlDirectCost != null && Math.Round(i.FtlDirectCost.Value, 2) >= searchDecimal - precision && Math.Round(i.FtlDirectCost.Value, 2) <= searchDecimal + precision
                || isDecimal && i.FtlRouteCost != null && Math.Round(i.FtlRouteCost.Value, 2) >= searchDecimal - precision && Math.Round(i.FtlRouteCost.Value, 2) <= searchDecimal + precision
                || isDecimal && i.LtlCost != null && Math.Round(i.LtlCost.Value, 2) >= searchDecimal - precision && Math.Round(i.LtlCost.Value, 2) <= searchDecimal + precision
                || isDecimal && i.MilkrunCost != null && Math.Round(i.MilkrunCost.Value, 2) >= searchDecimal - precision && Math.Round(i.MilkrunCost.Value, 2) <= searchDecimal + precision
                || isDecimal && i.PoolingCost != null && Math.Round(i.PoolingCost.Value, 2) >= searchDecimal - precision && Math.Round(i.PoolingCost.Value, 2) <= searchDecimal + precision
                || isDecimal && i.WeightKg != null && Math.Round(i.WeightKg.Value, 2) >= searchDecimal - precision && Math.Round(i.WeightKg.Value, 2) <= searchDecimal + precision

                || i.ShippingDate.HasValue && i.ShippingDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || i.DeliveryDate.HasValue && i.DeliveryDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                );
        }

        private MapperConfiguration ConfigureMapper()
        {
            var lang = _userProvider.GetCurrentUser()?.Language;
            var result = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AutogroupingShipping, AutogroupingShippingDto>()
                    .ForMember(x => x.ShippingDate, e => e.MapFrom((s, t) => s.ShippingDate.FormatDate()))
                    .ForMember(x => x.DeliveryDate, e => e.MapFrom((s, t) => s.DeliveryDate.FormatDate()))
                    .ForMember(x => x.CarrierId, e => e.Condition(s => s.CarrierId != null))
                    .ForMember(x => x.CarrierId, e => e.MapFrom((s, t) => new AutogroupingCarrierDto(s.CarrierId.FormatGuid(), s.Carrier?.ToString())))
                    .ForMember(x => x.VehicleTypeId, e => e.Condition(s => s.VehicleTypeId != null))
                    .ForMember(x => x.VehicleTypeId, e => e.MapFrom((s, t) => new LookUpDto(s.VehicleTypeId.FormatGuid(), s.VehicleType?.ToString())))
                    .ForMember(x => x.AutogroupingType, e => e.Condition(s => s.AutogroupingType != null))
                    .ForMember(x => x.AutogroupingType, e => e.MapFrom((s, t) => new LookUpDto(s.AutogroupingType.FormatEnum(), s.AutogroupingType.FormatEnum().Translate(lang))))
                    .ForMember(x => x.FtlDirectCost, e => e.MapFrom((s, t) => ConvertToRouteCostDto(s.FtlDirectCost, s.FtlDirectCostMessage, s.AutogroupingType == AutogroupingType.FtlDirect)))
                    .ForMember(x => x.FtlRouteCost, e => e.MapFrom((s, t) => ConvertToRouteCostDto(s.FtlRouteCost, s.FtlRouteCostMessage, s.AutogroupingType == AutogroupingType.FtlRoute)))
                    .ForMember(x => x.LtlCost, e => e.MapFrom((s, t) => ConvertToRouteCostDto(s.LtlCost, s.LtlCostMessage, s.AutogroupingType == AutogroupingType.Ltl)))
                    .ForMember(x => x.MilkrunCost, e => e.MapFrom((s, t) => ConvertToRouteCostDto(s.MilkrunCost, s.MilkrunCostMessage, s.AutogroupingType == AutogroupingType.Milkrun)))
                    .ForMember(x => x.PoolingCost, e => e.MapFrom((s, t) => ConvertToRouteCostDto(s.PoolingCost, s.PoolingCostMessage, s.AutogroupingType == AutogroupingType.Pooling)));

                cfg.CreateMap<AutogroupingOrder, AutogroupingOrderExportDto>()
                    .ForMember(x => x.ShippingDate, e => e.MapFrom((s, t) => s.ShippingDate.FormatDate()))
                    .ForMember(x => x.DeliveryDate, e => e.MapFrom((s, t) => s.DeliveryDate.FormatDate()))
                    .ForMember(x => x.ShippingWarehouseId, e => e.Condition(s => s.ShippingWarehouseId != null))
                    .ForMember(x => x.ShippingWarehouseId, e => e.MapFrom((s, t) => new LookUpDto(s.ShippingWarehouseId.FormatGuid(), s.ShippingWarehouse?.ToString())))
                    .ForMember(x => x.DeliveryWarehouseId, e => e.Condition(s => s.DeliveryWarehouseId != null))
                    .ForMember(x => x.DeliveryWarehouseId, e => e.MapFrom((s, t) => new LookUpDto(s.DeliveryWarehouseId.FormatGuid(), s.DeliveryWarehouse?.ToString())))
                    .ForMember(x => x.VehicleTypeId, e => e.Condition(s => s.VehicleTypeId != null))
                    .ForMember(x => x.VehicleTypeId, e => e.MapFrom((s, t) => new LookUpDto(s.VehicleTypeId.FormatGuid(), s.VehicleType?.ToString())))
                    .ForMember(x => x.BodyTypeId, e => e.Condition(s => s.BodyTypeId != null))
                    .ForMember(x => x.BodyTypeId, e => e.MapFrom((s, t) => new LookUpDto(s.BodyTypeId.FormatGuid(), s.BodyType?.ToString())))
                    .ForMember(x => x.ShippingNumber, e => e.Condition(s => s.AutogroupingShipping != null))
                    .ForMember(x => x.ShippingNumber, e => e.MapFrom((s, t) => s.AutogroupingShipping.ShippingNumber));
            });

            return result;
        }

        private RouteCostDto ConvertToRouteCostDto(decimal? value, string message, bool isSelected)
        {
            return new RouteCostDto
            {
                Value = value,
                Name = value == null ? "N/A" : null,
                Tooltip = message,
                Color = isSelected ? "blue" : null
            };
        }
        private Type GetReferenceType(PropertyInfo property)
        {
            var attr = property.GetCustomAttribute<ReferenceTypeAttribute>();
            return attr?.Type;
        }

        private List<LookUpDto> GetReferencedValues(IQueryable<AutogroupingShipping> query, Type refType, string field)
        {
            var ids = query.SelectField(field).Distinct();

            var result = _dataService.QueryAs<IPersistable>(refType)
                .Where(i => ids.Contains(i.Id))
                .ToList();

            return result.Select(i => new LookUpDto
            {
                Name = i.ToString(),
                Value = i.Id.FormatGuid()
            })
            .ToList();
        }

        private List<ColoredLookUpDto> GetStateValues(IQueryable<AutogroupingShipping> query, PropertyInfo propertyInfo)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var getMethod = typeof(Domain.Extensions.Extensions)
               .GetMethod(nameof(Domain.Extensions.Extensions.GetColor))
               .MakeGenericMethod(propertyInfo.PropertyType);

            var result = query.Select(i => propertyInfo.GetValue(i))
                .Where(i => i != null)
                .Distinct()
                .Select(i => new ColoredLookUpDto
                {
                    Name = i.FormatEnum(),
                    Value = i.FormatEnum(),
                    Color = getMethod.Invoke(i, new object[] { i }).FormatEnum()
                })
                .ToList();

            return result;
        }

        List<LookUpDto> GetSelectValues(IQueryable<AutogroupingShipping> query, PropertyInfo propertyInfo, bool showRawName)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            var result = query.Select(i => propertyInfo.GetValue(i))
                .Where(i => i != null)
                .Distinct()
                .ToList();

            return result.Select(i => new LookUpDto
            {
                Name = showRawName ? i.ToString() : i.FormatEnum().Translate(lang),
                Value = i.ToString()
            })
            .ToList();
        }
    }
}
