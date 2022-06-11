using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Orders;
using Application.Shared.Shippings;
using Application.Shared.Triggers;
using AutoMapper;
using DAL.Extensions;
using DAL.Queries;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.AppConfiguration;
using Domain.Services.FieldProperties;
using Domain.Services.History;
using Domain.Services.Orders;
using Domain.Services.Shippings;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Shippings
{
    public class ShippingsService : GridService<Shipping, ShippingDto, ShippingFormDto, ShippingSummaryDto, ShippingFilterDto>, IShippingsService
    {
        private readonly IMapper _mapper;
        private readonly IHistoryService _historyService;
        private readonly IShippingActionService _shippingActionService;
        private readonly ISendShippingService _sendShippingService;
        private readonly IShippingChangesService _shippingChangesService;
        private readonly IOrderChangesService _orderChangesService;

        public ShippingsService(
            IHistoryService historyService,
            ICommonDataService dataService,
            IUserProvider userIdProvider,
            IFieldDispatcherService fieldDispatcherService,
            IFieldPropertiesService fieldPropertiesService,
            IServiceProvider serviceProvider,
            ITriggersService triggersService,
            IValidationService validationService,
            IShippingActionService shippingActionService,
            ISendShippingService sendShippingService,
            IShippingChangesService shippingChangesService,
            IOrderChangesService orderChangesService,
            IEnumerable<IValidationRule<ShippingDto, Shipping>> validationRules)
            : base(dataService, userIdProvider, fieldDispatcherService, fieldPropertiesService, serviceProvider, triggersService, validationService, validationRules)
        {
            _mapper = ConfigureMapper().CreateMapper();
            _historyService = historyService;
            _shippingActionService = shippingActionService;
            _sendShippingService = sendShippingService;
            _shippingChangesService = shippingChangesService;
            _orderChangesService = orderChangesService;
        }

        public CreateShippingDto DefaultCreateForm()
        {
            var result = new CreateShippingDto { Orders = new List<CreateShippingOrderDto>() };

            var currentUser = _userIdProvider.GetCurrentUser();
            var companyId = currentUser?.CompanyId;
            var lang = currentUser?.Language;

            if (companyId != null)
            {
                var company = _dataService.GetById<Company>(companyId.Value);
                if (company?.NewShippingTarifficationType != null)
                {
                    var taiffTypeValue = company.NewShippingTarifficationType.FormatEnum();
                    result.TarifficationType = new LookUpDto(taiffTypeValue, taiffTypeValue.Translate(lang));
                }

                var shippingWarehouses = _dataService.GetDbSet<ShippingWarehouse>().Where(x => x.CompanyId == companyId).ToList();
                if (shippingWarehouses.Count == 1)
                {
                    var warehouse = shippingWarehouses.First();
                    result.ShippingWarehouseId = new LookUpDto(warehouse.Id.FormatGuid(), warehouse.ToString());
                    result.ShippingAddress = warehouse.Address;
                }
            }

            return result;
        }

        public ValidateResult Create(CreateShippingDto dto)
        {
            var lang = _userIdProvider.GetCurrentUser()?.Language;

            var result = ValidateCreateShippingDto(dto);
            if (result.IsError)
            {
                return result;
            }

            List<Order> orders = CreateNewShippingOrders(dto);

            var shipping = _shippingActionService.UnionOrders(orders);

            shipping.PoolingProductType = dto.PoolingProductType?.Value?.ToEnum<PoolingProductType>() ?? shipping.PoolingProductType;
            shipping.RouteNumber = dto.RouteNumber;
            var orderNumbers = string.Join(", ", orders.Select(x => x.OrderNumber));
            var resultMessage = "NewShippingSuccessMessage".Translate(lang, shipping.ShippingNumber, orderNumbers);

            if (shipping.TarifficationType == TarifficationType.Pooling
                || shipping.TarifficationType == TarifficationType.Milkrun)
            {
                var user = _userIdProvider.GetCurrentUser();
                var sendResult = _sendShippingService.SendShippingToPooling(user, shipping, orders);
                if (sendResult.IsError)
                {
                    _historyService.Save(shipping.Id, "NewShippingSendRequestError", sendResult.Message);

                    var errorMessage = "NewShippingSendRequestError".Translate(lang, sendResult.Message);
                    resultMessage = $"{resultMessage}. {errorMessage}";
                }
            }
            else
            {
                _sendShippingService.SendShippingToTk(shipping, orders);
            }

            var triggerResult = _triggersService.Execute(true);
            if (triggerResult.IsError)
            {
                return triggerResult;
            }

            _dataService.SaveChanges();

            return new ValidateResult(resultMessage, shipping.Id, false);
        }

        public UserConfigurationGridItem GetCreateFormConfiguration()
        {
            var companyId = _userIdProvider.GetCurrentUser()?.CompanyId;

            var columns = new List<UserConfigurationGridColumn>();
            var fields = _fieldDispatcherService.GetDtoFields<CreateShippingDto>().ToList();
            var orderFields = _fieldDispatcherService.GetDtoFields<CreateShippingOrderDto>();
            foreach (var orderField in orderFields)
            {
                if (!fields.Any(x => x.Name == orderField.Name))
                {
                    fields.Add(orderField);
                }
            }

            foreach (var field in fields)
            {
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

        private DetailedValidationResult ValidateCreateShippingDto(CreateShippingDto dto)
        {
            var currentUser = _userIdProvider.GetCurrentUser();
            var companyId = currentUser?.CompanyId;
            var lang = currentUser?.Language;
            var result = _validationService.Validate(dto);

            if (dto.DistributeDataByOrders == true)
            {
                if (dto.TotalWeightKg == null)
                {
                    result.AddError(nameof(dto.TotalWeightKg).ToLowerFirstLetter(),
                        "ValueIsRequired".Translate(lang, nameof(dto.TotalWeightKg).ToLowerFirstLetter().Translate(lang)),
                        ValidationErrorType.ValueIsRequired);
                }
                if (dto.TotalOrderAmountExcludingVAT == null)
                {
                    result.AddError(nameof(dto.TotalOrderAmountExcludingVAT).ToLowerFirstLetter(),
                        "ValueIsRequired".Translate(lang, nameof(dto.TotalOrderAmountExcludingVAT).ToLowerFirstLetter().Translate(lang)),
                        ValidationErrorType.ValueIsRequired);
                }
            }

            if (dto.Orders == null || !dto.Orders.Any())
            {
                result.AddError(nameof(dto.Orders).ToLowerFirstLetter(),
                    "EmptyOrdersForNewShippingError".Translate(lang),
                    ValidationErrorType.ValueIsRequired);
            }
            else
            {
                int ind = 0;

                var orderNumbers = dto.Orders.Select(x => x.OrderNumber).Where(x => !string.IsNullOrEmpty(x)).ToList();
                var previousNumbers = _dataService.GetDbSet<Order>()
                                                  .Where(x => x.CompanyId == companyId && orderNumbers.Contains(x.OrderNumber))
                                                  .Select(x => x.OrderNumber)
                                                  .ToHashSet();

                foreach (var orderData in dto.Orders)
                {
                    var orderResult = _validationService.Validate(orderData);
                    if (orderResult.IsError)
                    {
                        foreach (var error in orderResult.Errors)
                        {
                            error.Name = $"{error.Name}_{ind}";
                            result.AddError(error);
                        }
                    }

                    if (!string.IsNullOrEmpty(orderData.OrderNumber))
                    {
                        if (previousNumbers.Contains(orderData.OrderNumber))
                        {
                            result.AddError($"{nameof(orderData.OrderNumber).ToLowerFirstLetter()}_{ind}",
                                "NewShippingOrder.DuplicatedRecord".Translate(lang),
                                ValidationErrorType.DuplicatedRecord);
                        }
                        previousNumbers.Add(orderData.OrderNumber);
                    }

                    if (dto.DistributeDataByOrders != true)
                    {
                        if (orderData.WeightKg == null)
                        {
                            result.AddError($"{nameof(orderData.WeightKg).ToLowerFirstLetter()}_{ind}",
                                "ValueIsRequired".Translate(lang, nameof(orderData.WeightKg).ToLowerFirstLetter().Translate(lang)),
                                ValidationErrorType.ValueIsRequired);
                        }
                        if (orderData.OrderAmountExcludingVAT == null)
                        {
                            result.AddError($"{nameof(orderData.OrderAmountExcludingVAT).ToLowerFirstLetter()}_{ind}",
                                "ValueIsRequired".Translate(lang, nameof(orderData.OrderAmountExcludingVAT).ToLowerFirstLetter().Translate(lang)),
                                ValidationErrorType.ValueIsRequired);
                        }
                    }

                    ind++;
                }
            }

            return result;
        }

        private List<Order> CreateNewShippingOrders(CreateShippingDto dto)
        {
            var orderDtos = new List<OrderDto>();
            var palletOccurrences = new Dictionary<int, int>();
            if (dto.Orders != null)
            {
                foreach (var orderData in dto.Orders)
                {
                    if (orderData.PalletsFrom != null && orderData.PalletsTo != null)
                    {
                        for (int palletNum = orderData.PalletsFrom.Value; palletNum <= orderData.PalletsTo.Value; palletNum++)
                        {
                            if (!palletOccurrences.ContainsKey(palletNum))
                                palletOccurrences[palletNum] = 0;
                            ++palletOccurrences[palletNum];
                        }
                    }
                }

                var deliveryWarehouseId = dto.DeliveryWarehouseId?.Value.ToGuid();
                var deliveryWarehouse = deliveryWarehouseId == null ? null : _dataService.GetById<Warehouse>(deliveryWarehouseId.Value);

                foreach (var orderData in dto.Orders)
                {
                    decimal? palletsCount = null;
                    if (orderData.PalletsFrom != null && orderData.PalletsTo != null)
                    {
                        for (int palletNum = orderData.PalletsFrom.Value; palletNum <= orderData.PalletsTo.Value; palletNum++)
                        {
                            palletsCount = (palletsCount ?? 0M) + (1M / palletOccurrences[palletNum]);
                        }
                    }

                    var orderDto = new OrderDto
                    {
                        BodyTypeId = dto.BodyTypeId,
                        CarrierId = dto.CarrierId,
                        ClientOrderNumber = orderData.ClientOrderNumber,
                        DeliveryAddress = dto.DeliveryAddress,
                        DeliveryDate = dto.DeliveryDate,
                        DeliveryWarehouseId = dto.DeliveryWarehouseId,
                        OrderAmountExcludingVAT = orderData.OrderAmountExcludingVAT,
                        OrderNumber = orderData.OrderNumber,
                        OrderType = orderData.OrderType,
                        PalletsCount = palletsCount,
                        ShippingAddress = dto.ShippingAddress,
                        ShippingDate = dto.ShippingDate,
                        ShippingWarehouseId = dto.ShippingWarehouseId,
                        TarifficationType = dto.TarifficationType,
                        WeightKg = orderData.WeightKg,
                        BottlesCount = dto.BottlesCount,
                        Volume9l = dto.Volume9l
                    };

                    if (orderDto.ShippingDate == null)
                    {
                        orderDto.ShippingDate = orderDto.DeliveryDate.ToDateTime()?.AddDays(-deliveryWarehouse?.LeadtimeDays ?? 0).FormatDateTime();
                    }

                    orderDtos.Add(orderDto);
                }
            }

            var totalPallets = orderDtos.Sum(x => x.PalletsCount ?? 0M);
            if (totalPallets > 0 && dto.DistributeDataByOrders == true)
            {
                foreach (var orderDto in orderDtos)
                {
                    orderDto.WeightKg = dto.TotalWeightKg * orderDto.PalletsCount / totalPallets;
                    orderDto.OrderAmountExcludingVAT = dto.TotalOrderAmountExcludingVAT * orderDto.PalletsCount / totalPallets;
                }
            }

            var orderDbSet = _dataService.GetDbSet<Order>();
            var orders = new List<Order>();
            foreach (var orderDto in orderDtos)
            {
                var orderEntity = new Order
                {
                    Id = Guid.NewGuid()
                };

                _orderChangesService.MapFromDtoToEntity(orderEntity, orderDto);

                orderDbSet.Add(orderEntity);
                orders.Add(orderEntity);
            }

            return orders;
        }

        protected override IQueryable<Shipping> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.BodyType)
                .Include(i => i.VehicleType)
                .Include(i => i.Carrier)
                .Include(i => i.ShippingWarehouse)
                .Include(i => i.DeliveryWarehouse)
                .Include(i => i.Company);
        }

        public override LookUpDto MapFromEntityToLookupDto(Shipping entity)
        {
            return new LookUpDto
            {
                Value = entity.Id.FormatGuid(),
                Name = entity.ShippingNumber
            };
        }

        protected override void OnGetForm(Shipping entity, Role role)
        {
            bool hasChanges = _shippingChangesService.ClearBacklightFlags(new[] { entity }, role);
            if (hasChanges)
            {
                _dataService.SaveChanges();
            }
        }

        public override ShippingSummaryDto GetSummary(IEnumerable<Guid> ids)
        {
            return new ShippingSummaryDto();
        }

        public IEnumerable<LookUpDto> FindByNumber(NumberSearchFormDto dto)
        {
            var dbSet = _dataService.GetDbSet<Shipping>();
            List<Shipping> entities;
            if (dto.IsPartial)
            {
                entities = dbSet.Where(x => x.ShippingNumber.Contains(dto.Number, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
            else
            {
                entities = dbSet.Where(x => x.ShippingNumber == dto.Number).ToList();
            }
            var result = entities.Select(MapFromEntityToLookupDto);
            return result;
        }

        protected override IQueryable<Shipping> ApplyRestrictions(IQueryable<Shipping> query)
        {
            query = base.ApplyRestrictions(query);

            var currentUserId = _userIdProvider.GetCurrentUserId();
            var user = _dataService.GetDbSet<User>().GetById(currentUserId.Value);

            if (user.CarrierId.HasValue)
            {
                var invalidShippingStates = new[] { ShippingState.ShippingCreated, ShippingState.ShippingCanceled };

                query = query
                    .Where(x => x.CarrierId == user.CarrierId)
                    .Where(i => !i.Status.HasValue || !invalidShippingStates.Contains(i.Status.Value));
            }

            return query;
        }

        public override IEnumerable<EntityStatusDto<Shipping>> LoadStatusData(IEnumerable<Guid> ids)
        {
            var result = _dataService.GetDbSet<Shipping>()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new EntityStatusDto<Shipping>
                {
                    Id = x.Id.FormatGuid(),
                    Status = x.Status.ToString(),
                    Entity = x
                })
                .ToList();

            return result;
        }

        private MapperConfiguration ConfigureMapper()
        {
            var lang = _userIdProvider.GetCurrentUser()?.Language;

            var result = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ShippingDto, Shipping>()
                    .ForMember(t => t.Id, e => e.Ignore())
                    .ForMember(t => t.Status, e => e.Ignore())
                    .ForMember(t => t.ManualActualPalletsCount, e => e.Ignore())
                    .ForMember(t => t.ManualActualWeightKg, e => e.Ignore())
                    .ForMember(t => t.ManualConfirmedPalletsCount, e => e.Ignore())
                    .ForMember(t => t.ManualPalletsCount, e => e.Ignore())
                    .ForMember(t => t.ManualTrucksDowntime, e => e.Ignore())
                    .ForMember(t => t.ManualWeightKg, e => e.Ignore())
                    .ForMember(t => t.IsNewCarrierRequest, e => e.Ignore())
                    .ForMember(t => t.DeliveryType, e => e.MapFrom((s) => s.DeliveryType == null || string.IsNullOrEmpty(s.DeliveryType.Value) ? (DeliveryType?)null : MapFromStateDto<DeliveryType>(s.DeliveryType.Value)))
                    .ForMember(t => t.TarifficationType, e => e.MapFrom((s) => s.TarifficationType == null || string.IsNullOrEmpty(s.TarifficationType.Value) ? (TarifficationType?)null : MapFromStateDto<TarifficationType>(s.TarifficationType.Value)))
                    .ForMember(t => t.PoolingProductType, e => e.MapFrom((s) => s.PoolingProductType == null || string.IsNullOrEmpty(s.PoolingProductType.Value) ? (PoolingProductType?)null : MapFromStateDto<PoolingProductType>(s.PoolingProductType.Value)))
                    .ForMember(t => t.CarrierId, e => e.MapFrom((s) => s.CarrierId == null ? null : s.CarrierId.Value.ToGuid()))
                    .ForMember(t => t.VehicleTypeId, e => e.MapFrom((s) => s.VehicleTypeId == null ? null : s.VehicleTypeId.Value.ToGuid()))
                    .ForMember(t => t.BodyTypeId, e => e.MapFrom((s) => s.BodyTypeId == null ? null : s.BodyTypeId.Value.ToGuid()))
                    .ForMember(t => t.ShippingWarehouseId, e => e.MapFrom((s) => s.ShippingWarehouseId == null ? null : s.ShippingWarehouseId.Value.ToGuid()))
                    .ForMember(t => t.DeliveryWarehouseId, e => e.MapFrom((s) => s.DeliveryWarehouseId == null ? null : s.DeliveryWarehouseId.Value.ToGuid()))
                    .ForMember(t => t.CompanyId, e => e.Condition((s) => s.CompanyId != null))
                    .ForMember(t => t.CompanyId, e => e.MapFrom((s) => s.CompanyId.Value.ToGuid()))
                    .ForMember(t => t.LoadingArrivalTime, e => e.MapFrom((s) => s.LoadingArrivalTime.ToDateTime()))
                    .ForMember(t => t.LoadingDepartureTime, e => e.MapFrom((s) => s.LoadingDepartureTime.ToDateTime()))
                    .ForMember(t => t.ShippingDate, e => e.MapFrom((s) => s.ShippingDate.ToDateTime()))
                    .ForMember(t => t.DeliveryDate, e => e.MapFrom((s) => s.DeliveryDate.ToDateTime()))
                    .ForMember(t => t.BlankArrival, e => e.MapFrom((s) => s.BlankArrival))
                    .ForMember(t => t.Waybill, e => e.MapFrom((s) => s.Waybill))
                    .ForMember(t => t.WaybillTorg12, e => e.MapFrom((s) => s.WaybillTorg12))
                    .ForMember(t => t.TransportWaybill, e => e.MapFrom((s) => s.TransportWaybill))
                    .ForMember(t => t.Invoice, e => e.MapFrom((s) => s.Invoice))
                    .ForMember(t => t.DocumentsReturnDate, e => e.MapFrom((s) => s.DocumentsReturnDate.ToDate()))
                    .ForMember(t => t.ActualDocumentsReturnDate, e => e.MapFrom((s) => s.ActualDocumentsReturnDate.ToDate()));

                cfg.CreateMap<ShippingDto, ShippingFormDto>();

                cfg.CreateMap<Shipping, ShippingDto>()
                    .ForMember(t => t.Id, e => e.MapFrom((s, t) => s.Id.FormatGuid()))
                    .ForMember(t => t.Status, e => e.MapFrom((s, t) => s.Status.FormatEnum()))
                    .ForMember(t => t.DeliveryType, e => e.MapFrom((s, t) => s.DeliveryType == null ? null : s.DeliveryType.GetEnumLookup(lang)))
                    .ForMember(t => t.CarrierId, e => e.MapFrom((s, t) => s.Carrier == null ? null : new LookUpDto(s.CarrierId.FormatGuid(), s.Carrier.ToString())))
                    .ForMember(t => t.VehicleTypeId, e => e.MapFrom((s, t) => s.VehicleType == null ? null : new LookUpDto(s.VehicleTypeId.FormatGuid(), s.VehicleType.ToString())))
                    .ForMember(t => t.BodyTypeId, e => e.MapFrom((s, t) => s.BodyType == null ? null : new LookUpDto(s.BodyTypeId.FormatGuid(), s.BodyType.ToString())))
                    .ForMember(t => t.CompanyId, e => e.MapFrom((s, t) => s.CompanyId == null ? null : new LookUpDto(s.CompanyId.FormatGuid(), s.Company.ToString())))
                    .ForMember(t => t.ShippingWarehouseId, e => e.MapFrom((s, t) => s.ShippingWarehouse == null ? null : new LookUpDto(s.ShippingWarehouseId.FormatGuid(), s.ShippingWarehouse.ToString())))
                    .ForMember(t => t.DeliveryWarehouseId, e => e.MapFrom((s, t) => s.DeliveryWarehouse == null ? null : new LookUpDto(s.DeliveryWarehouseId.FormatGuid(), s.DeliveryWarehouse.ToString())))
                    .ForMember(t => t.TarifficationType, e => e.MapFrom((s, t) => s.TarifficationType == null ? null : s.TarifficationType.GetEnumLookup(lang)))
                    .ForMember(t => t.PoolingProductType, e => e.MapFrom((s, t) => s.PoolingProductType == null ? null : s.PoolingProductType.GetEnumLookup(lang)))
                    .ForMember(t => t.LoadingArrivalTime, e => e.MapFrom((s, t) => s.LoadingArrivalTime?.FormatDateTime()))
                    .ForMember(t => t.LoadingDepartureTime, e => e.MapFrom((s, t) => s.LoadingDepartureTime?.FormatDateTime()))
                    .ForMember(t => t.ShippingDate, e => e.MapFrom((s, t) => s.ShippingDate?.FormatDateTime()))
                    .ForMember(t => t.DeliveryDate, e => e.MapFrom((s, t) => s.DeliveryDate?.FormatDateTime()))
                    .ForMember(t => t.DocumentsReturnDate, e => e.MapFrom((s, t) => s.DocumentsReturnDate?.FormatDate()))
                    .ForMember(t => t.ActualDocumentsReturnDate, e => e.MapFrom((s, t) => s.ActualDocumentsReturnDate?.FormatDate()));
            });
            return result;
        }

        public override void MapFromDtoToEntity(Shipping entity, ShippingDto dto)
        {
            bool isNew = string.IsNullOrEmpty(dto.Id);

            IEnumerable<string> readOnlyFields = null;
            if (!isNew)
            {
                var userId = _userIdProvider.GetCurrentUserId();
                if (userId != null)
                {
                    string stateName = entity.Status.FormatEnum();
                    readOnlyFields = _fieldPropertiesService.GetReadOnlyFields(FieldPropertiesForEntityType.Shippings, stateName, entity.CompanyId, null, userId);
                }
            }

            _mapper.Map(dto, entity);
        }

        public override void MapFromFormDtoToEntity(Shipping entity, ShippingFormDto dto)
        {
            MapFromDtoToEntity(entity, dto);

            var orders = _dataService.GetDbSet<Order>().Where(o => o.ShippingId == entity.Id).ToList();

            SaveRoutePoints(entity, orders, dto);
            SaveOrderCosts(entity, orders, dto);
        }

        public override ShippingDto MapFromEntityToDto(Shipping entity, Role role)
        {
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<ShippingDto>(entity);
        }

        public override ShippingFormDto MapFromEntityToFormDto(Shipping entity)
        {
            if (entity == null)
            {
                return null;
            }

            ShippingDto dto = MapFromEntityToDto(entity, GetRole());
            ShippingFormDto formDto = _mapper.Map<ShippingFormDto>(dto);

            var orders = _dataService.GetDbSet<Order>()
                                     .Include(x => x.ShippingWarehouse)
                                     .Include(x => x.DeliveryWarehouse)
                                     .Where(o => o.ShippingId == entity.Id)
                                     .ToList();

            formDto.Orders = GetShippingOrders(orders);
            formDto.RoutePoints = GetRoutePoints(entity, orders);
            formDto.OrderCosts = GetOrderCosts(orders);

            return formDto;
        }

        private ValidateResult SaveRoutePoints(Shipping entity, List<Order> orders, ShippingFormDto dto)
        {
            if (dto.RoutePoints != null)
            {
                var ordersDict = orders.ToDictionary(o => o.Id.FormatGuid());

                foreach (RoutePointDto pointDto in dto.RoutePoints)
                {
                    if (pointDto.OrderIds == null)
                    {
                        continue;
                    }

                    foreach (string orderId in pointDto.OrderIds)
                    {
                        Order order;
                        if (ordersDict.TryGetValue(orderId, out order))
                        {
                            if (pointDto.IsLoading)
                            {
                                order.ShippingDate = pointDto.PlannedDate.ToDateTime();

                                order.LoadingArrivalTime = pointDto.ArrivalTime.ToDateTime();
                                order.LoadingDepartureTime = pointDto.DepartureTime.ToDateTime();

                                if (!string.IsNullOrEmpty(pointDto.VehicleStatus))
                                    order.ShippingStatus = MapFromStateDto<VehicleState>(pointDto.VehicleStatus);
                            }
                            else
                            {
                                order.DeliveryDate = pointDto.PlannedDate.ToDateTime();

                                order.UnloadingArrivalTime = pointDto.ArrivalTime.ToDateTime();
                                order.UnloadingDepartureTime = pointDto.DepartureTime.ToDateTime();

                                order.TrucksDowntime = pointDto.TrucksDowntime;
                                if (!string.IsNullOrEmpty(pointDto.VehicleStatus))
                                    order.DeliveryStatus = MapFromStateDto<VehicleState>(pointDto.VehicleStatus);
                            }
                        }
                    }
                }

                var loadingArrivalTimes = orders
                    .Where(i => i.LoadingArrivalTime != null)
                    .Select(i => i.LoadingArrivalTime);
                var loadingArrivalTime = loadingArrivalTimes.Any() ? loadingArrivalTimes.Min() : null;

                var loadingDepartureTimes = orders
                    .Where(i => i.LoadingDepartureTime != null)
                    .Select(i => i.LoadingDepartureTime);
                var loadingDepartureTime = loadingDepartureTimes.Any() ? loadingDepartureTimes.Min() : null;

                entity.LoadingArrivalTime = loadingArrivalTime;
                entity.LoadingDepartureTime = loadingDepartureTime;
            }

            return new ValidateResult(entity.Id);
        }

        private ValidateResult SaveOrderCosts(Shipping entity, List<Order> orders, ShippingFormDto dto)
        {
            if (dto.OrderCosts != null)
            {
                var ordersDict = orders.ToDictionary(o => o.Id.FormatGuid());
                foreach (var orderCost in dto.OrderCosts.Where(x => !string.IsNullOrEmpty(x.Id)))
                {
                    if (ordersDict.TryGetValue(orderCost.Id, out Order order))
                    {
                        if (order.ReturnShippingCost != orderCost.ReturnCostWithoutVAT)
                        {
                            order.ReturnShippingCost = orderCost.ReturnCostWithoutVAT;
                            _historyService.Save(entity.Id, "ReturnShippingCostOrderShippingSetter", order.OrderNumber, order.ReturnShippingCost);
                        }
                    }
                }

                var ordersToUpdate = dto.OrderCosts.Select(x => x.Id.ToGuid()).ToHashSet();
                foreach (var order in orders)
                {
                    if (!ordersToUpdate.Contains(order.Id))
                    {
                        if (order.ReturnShippingCost != null)
                        {
                            order.ReturnShippingCost = null;
                            _historyService.Save(entity.Id, "ReturnShippingCostOrderShippingSetter", order.OrderNumber, order.ReturnShippingCost);
                        }
                    }
                }

                entity.ReturnCostWithoutVAT = orders.Sum(x => x.ReturnShippingCost ?? 0M);
            }
            return new ValidateResult(entity.Id);
        }

        private List<ShippingOrderDto> GetShippingOrders(List<Order> orders)
        {
            List<ShippingOrderDto> result = new List<ShippingOrderDto>();
            foreach (Order order in orders.OrderBy(o => o.OrderNumber))
            {
                ShippingOrderDto dto = new ShippingOrderDto
                {
                    Id = order.Id.FormatGuid(),
                    OrderNumber = order.OrderNumber,
                    Status = order.Status.FormatEnum()
                };
                result.Add(dto);
            }
            return result;
        }

        private List<OrderCostDto> GetOrderCosts(List<Order> orders)
        {
            List<OrderCostDto> result = new List<OrderCostDto>();
            foreach (Order order in orders.Where(x => x.ReturnShippingCost != null).OrderBy(o => o.OrderNumber))
            {
                OrderCostDto dto = new OrderCostDto
                {
                    Id = order.Id.FormatGuid(),
                    OrderNumber = order.OrderNumber,
                    ReturnCostWithoutVAT = order.ReturnShippingCost
                };
                result.Add(dto);
            }
            return result;
        }

        private List<RoutePointDto> GetRoutePoints(Shipping entity, List<Order> orders)
        {
            var points = new Dictionary<string, RoutePointDto>();
            foreach (Order order in orders)
            {
                RoutePointDto loadingPoint;
                string loadingPointKey = $"L-{order.ShippingWarehouseId.FormatGuid() ?? order.ShippingAddress}-{order.ShippingDate.FormatDateTime()}";
                if (!points.TryGetValue(loadingPointKey, out loadingPoint))
                {
                    loadingPoint = new RoutePointDto
                    {
                        WarehouseName = order.ShippingWarehouse?.WarehouseName ?? order.ShippingCity,
                        Address = order.ShippingAddress,
                        PlannedDate = order.ShippingDate.FormatDateTime(),
                        ArrivalTime = order.LoadingArrivalTime.FormatDateTime(),
                        DepartureTime = order.LoadingDepartureTime.FormatDateTime(),
                        VehicleStatus = order.ShippingStatus.FormatEnum(),
                        TrucksDowntime = null,
                        IsLoading = true,
                        OrderIds = new List<string>()
                    };
                    points[loadingPointKey] = loadingPoint;
                }
                loadingPoint.OrderIds.Add(order.Id.FormatGuid());

                RoutePointDto unloadingPoint;
                string unloadingPointKey = $"U-{order.DeliveryWarehouseId.FormatGuid() ?? order.DeliveryAddress}-{order.DeliveryDate.FormatDateTime()}";
                if (!points.TryGetValue(unloadingPointKey, out unloadingPoint))
                {
                    unloadingPoint = new RoutePointDto
                    {
                        WarehouseName = order.DeliveryWarehouse?.Client ?? order.DeliveryCity,
                        Address = order.DeliveryAddress,
                        PlannedDate = order.DeliveryDate.FormatDateTime(),
                        ArrivalTime = order.UnloadingArrivalTime.FormatDateTime(),
                        DepartureTime = order.UnloadingDepartureTime.FormatDateTime(),
                        VehicleStatus = order.DeliveryStatus.FormatEnum(),
                        TrucksDowntime = order.TrucksDowntime,
                        IsLoading = false,
                        OrderIds = new List<string>()
                    };
                    points[unloadingPointKey] = unloadingPoint;
                }
                unloadingPoint.OrderIds.Add(order.Id.FormatGuid());
            }

            var pointsList = points.Values.OrderBy(p => p.PlannedDate.ToDateTime())
                                          .ThenBy(p => p.IsLoading ? 0 : 1)
                                          .ThenBy(p => p.VehicleStatus)
                                          .ThenBy(p => p.WarehouseName)
                                          .ToList();
            return pointsList;
        }

        protected override IQueryable<Shipping> ApplySort(IQueryable<Shipping> query, FilterFormDto<ShippingFilterDto> searchForm)
        {
            return query.OrderBy(searchForm.Sort?.Name, searchForm.Sort?.Desc)
                .DefaultOrderBy(i => i.ShippingCreationDate, !string.IsNullOrEmpty(searchForm.Sort?.Name), true)
                .DefaultOrderBy(i => i.Id, true);
        }

        public override IQueryable<Shipping> ApplySearchForm(IQueryable<Shipping> query, FilterFormDto<ShippingFilterDto> searchForm, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            where = where
                         .WhereAnd(searchForm.Filter.ActualDocumentsReturnDate.ApplyDateRangeFilter<Shipping>(i => i.ActualDocumentsReturnDate, ref parameters))
                         .WhereAnd(searchForm.Filter.ActualPalletsCount.ApplyNumericFilter<Shipping>(i => i.ActualPalletsCount, ref parameters))
                         .WhereAnd(searchForm.Filter.ActualWeightKg.ApplyNumericFilter<Shipping>(i => i.ActualWeightKg, ref parameters))
                         .WhereAnd(searchForm.Filter.AdditionalCostsComments.ApplyStringFilter<Shipping>(i => i.AdditionalCostsComments, ref parameters))
                         .WhereAnd(searchForm.Filter.AdditionalCostsWithoutVAT.ApplyNumericFilter<Shipping>(i => i.AdditionalCostsWithoutVAT, ref parameters))
                         .WhereAnd(searchForm.Filter.ExtraPointCostsWithoutVAT.ApplyNumericFilter<Shipping>(i => i.ExtraPointCostsWithoutVAT, ref parameters))
                         .WhereAnd(searchForm.Filter.AdditionalPointRate.ApplyNumericFilter<Shipping>(i => i.AdditionalPointRate, ref parameters))
                         .WhereAnd(searchForm.Filter.BlankArrival.ApplyBooleanFilter<Shipping>(i => i.BlankArrival, ref parameters))
                         .WhereAnd(searchForm.Filter.BlankArrivalRate.ApplyNumericFilter<Shipping>(i => i.BlankArrivalRate, ref parameters))
                         .WhereAnd(searchForm.Filter.CarrierId.ApplyOptionsFilter<Shipping, Guid?>(i => i.CarrierId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.CompanyId.ApplyOptionsFilter<Shipping, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.ConfirmedPalletsCount.ApplyNumericFilter<Shipping>(i => i.ConfirmedPalletsCount, ref parameters))
                         .WhereAnd(searchForm.Filter.CostsComments.ApplyStringFilter<Shipping>(i => i.CostsComments, ref parameters))
                         .WhereAnd(searchForm.Filter.CostsConfirmedByCarrier.ApplyBooleanFilter<Shipping>(i => i.CostsConfirmedByCarrier, ref parameters))
                         .WhereAnd(searchForm.Filter.CostsConfirmedByShipper.ApplyBooleanFilter<Shipping>(i => i.CostsConfirmedByShipper, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryAddress.ApplyStringFilter<Shipping>(i => i.DeliveryAddress, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryInvoiceNumber.ApplyStringFilter<Shipping>(i => i.DeliveryInvoiceNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryType.ApplyEnumFilter<Shipping, DeliveryType>(i => i.DeliveryType, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryCostWithoutVAT.ApplyNumericFilter<Shipping>(i => i.DeliveryCostWithoutVAT, ref parameters))
                         .WhereAnd(searchForm.Filter.DeviationReasonsComments.ApplyStringFilter<Shipping>(i => i.DeviationReasonsComments, ref parameters))
                         .WhereAnd(searchForm.Filter.DriverName.ApplyStringFilter<Shipping>(i => i.DriverName, ref parameters))
                         .WhereAnd(searchForm.Filter.DriverPhone.ApplyStringFilter<Shipping>(i => i.DriverPhone, ref parameters))
                         .WhereAnd(searchForm.Filter.DriverPassportData.ApplyStringFilter<Shipping>(i => i.DriverPassportData, ref parameters))
                         .WhereAnd(searchForm.Filter.DriverPassportData.ApplyStringFilter<Shipping>(i => i.DriverPassportData, ref parameters))
                         .WhereAnd(searchForm.Filter.DocumentsReturnDate.ApplyDateRangeFilter<Shipping>(i => i.DocumentsReturnDate, ref parameters))
                         .WhereAnd(searchForm.Filter.DowntimeRate.ApplyNumericFilter<Shipping>(i => i.DowntimeRate, ref parameters))
                         .WhereAnd(searchForm.Filter.LoadingDowntimeCost.ApplyNumericFilter<Shipping>(i => i.LoadingDowntimeCost, ref parameters))
                         .WhereAnd(searchForm.Filter.UnloadingDowntimeCost.ApplyNumericFilter<Shipping>(i => i.UnloadingDowntimeCost, ref parameters))
                         .WhereAnd(searchForm.Filter.Invoice.ApplyBooleanFilter<Shipping>(i => i.Invoice, ref parameters))
                         .WhereAnd(searchForm.Filter.InvoiceAmountWithoutVAT.ApplyNumericFilter<Shipping>(i => i.InvoiceAmountWithoutVAT, ref parameters))
                         .WhereAnd(searchForm.Filter.InvoiceNumber.ApplyStringFilter<Shipping>(i => i.InvoiceNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.LoadingArrivalTime.ApplyDateRangeFilter<Shipping>(i => i.LoadingArrivalTime, ref parameters))
                         .WhereAnd(searchForm.Filter.LoadingDepartureTime.ApplyDateRangeFilter<Shipping>(i => i.LoadingDepartureTime, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingDate.ApplyDateRangeFilter<Shipping>(i => i.ShippingDate, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryDate.ApplyDateRangeFilter<Shipping>(i => i.DeliveryDate, ref parameters))
                         .WhereAnd(searchForm.Filter.OtherCosts.ApplyNumericFilter<Shipping>(i => i.OtherCosts, ref parameters))
                         .WhereAnd(searchForm.Filter.PalletsCount.ApplyNumericFilter<Shipping>(i => i.PalletsCount, ref parameters))
                         .WhereAnd(searchForm.Filter.ReturnCostWithoutVAT.ApplyNumericFilter<Shipping>(i => i.ReturnCostWithoutVAT, ref parameters))
                         .WhereAnd(searchForm.Filter.ReturnRate.ApplyNumericFilter<Shipping>(i => i.ReturnRate, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingAddress.ApplyStringFilter<Shipping>(i => i.ShippingAddress, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingCreationDate.ApplyDateRangeFilter<Shipping>(i => i.ShippingCreationDate, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingNumber.ApplyStringFilter<Shipping>(i => i.ShippingNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.Status.ApplyEnumFilter<Shipping, ShippingState>(i => i.Status, ref parameters))
                         .WhereAnd(searchForm.Filter.TarifficationType.ApplyEnumFilter<Shipping, TarifficationType>(i => i.TarifficationType, ref parameters))
                         .WhereAnd(searchForm.Filter.PoolingProductType.ApplyEnumFilter<Shipping, PoolingProductType>(i => i.PoolingProductType, ref parameters))
                         .WhereAnd(searchForm.Filter.TemperatureMax.ApplyNumericFilter<Shipping>(i => i.TemperatureMax, ref parameters))
                         .WhereAnd(searchForm.Filter.TemperatureMin.ApplyNumericFilter<Shipping>(i => i.TemperatureMin, ref parameters))
                         .WhereAnd(searchForm.Filter.TotalDeliveryCost.ApplyNumericFilter<Shipping>(i => i.TotalDeliveryCost, ref parameters))
                         .WhereAnd(searchForm.Filter.TotalDeliveryCostWithoutVAT.ApplyNumericFilter<Shipping>(i => i.TotalDeliveryCostWithoutVAT, ref parameters))
                         .WhereAnd(searchForm.Filter.BasicDeliveryCostWithoutVAT.ApplyNumericFilter<Shipping>(i => i.BasicDeliveryCostWithoutVAT, ref parameters))
                         .WhereAnd(searchForm.Filter.TransportWaybill.ApplyBooleanFilter<Shipping>(i => i.TransportWaybill, ref parameters))
                         .WhereAnd(searchForm.Filter.TrucksDowntime.ApplyNumericFilter<Shipping>(i => i.TrucksDowntime, ref parameters))
                         .WhereAnd(searchForm.Filter.VehicleMake.ApplyStringFilter<Shipping>(i => i.VehicleMake, ref parameters))
                         .WhereAnd(searchForm.Filter.VehicleNumber.ApplyStringFilter<Shipping>(i => i.VehicleNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.VehicleTypeId.ApplyOptionsFilter<Shipping, Guid?>(i => i.VehicleTypeId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.BodyTypeId.ApplyOptionsFilter<Shipping, Guid?>(i => i.BodyTypeId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.ShippingWarehouseId.ApplyOptionsFilter<Shipping, Guid?>(i => i.ShippingWarehouseId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.DeliveryWarehouseId.ApplyOptionsFilter<Shipping, Guid?>(i => i.DeliveryWarehouseId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.TrailerNumber.ApplyStringFilter<Shipping>(i => i.TrailerNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.Waybill.ApplyBooleanFilter<Shipping>(i => i.Waybill, ref parameters))
                         .WhereAnd(searchForm.Filter.WaybillTorg12.ApplyBooleanFilter<Shipping>(i => i.WaybillTorg12, ref parameters))
                         .WhereAnd(searchForm.Filter.WeightKg.ApplyNumericFilter<Shipping>(i => i.WeightKg, ref parameters))
                         .WhereAnd(searchForm.Filter.IsPooling.ApplyBooleanFilter<Shipping>(i => i.IsPooling, ref parameters))
                         .WhereAnd(searchForm.Filter.RouteNumber.ApplyStringFilter<Shipping>(i => i.RouteNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.BottlesCount.ApplyNumericFilter<Shipping>(i => i.BottlesCount, ref parameters))
                         .WhereAnd(searchForm.Filter.Volume9l.ApplyNumericFilter<Shipping>(i => i.Volume9l, ref parameters));

            string sql = $@"SELECT * FROM ""Shippings"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            // Apply Search
            return this.ApplySearch(query, searchForm?.Filter?.Search, columns ?? searchForm?.Filter?.Columns);
        }

        private IQueryable<Shipping> ApplySearch(IQueryable<Shipping> query, string search, List<string> columns)
        {
            if (string.IsNullOrEmpty(search)) return query;

            search = search.ToLower().Trim();

            var isInt = int.TryParse(search, out int searchInt);

            decimal? searchDecimal = search.ToDecimal();
            var isDecimal = searchDecimal != null;
            decimal precision = 0.01M;

            var searchDateFormat = "dd.mm.yyyy HH24:MI";

            var companyId = _userIdProvider.GetCurrentUser()?.CompanyId;

            //TarifficationType search

            var tarifficationTypeNames = Enum.GetNames(typeof(TarifficationType)).Select(i => i.ToLower());

            var tarifficationTypes = this._dataService.GetDbSet<Translation>()
                .Where(i => tarifficationTypeNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<TarifficationType>())
                .ToList();

            //PoolingProductType search

            var poolingProductTypeNames = Enum.GetNames(typeof(PoolingProductType)).Select(i => i.ToLower());

            var poolingProductTypes = this._dataService.GetDbSet<Translation>()
                .Where(i => poolingProductTypeNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<PoolingProductType>())
                .ToList();

            //DeliveryType search

            var deliveryTypeNames = Enum.GetNames(typeof(DeliveryType)).Select(i => i.ToLower());

            var deliveryTypes = this._dataService.GetDbSet<Translation>()
                .Where(i => deliveryTypeNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<DeliveryType>())
                .ToList();

            var statusNames = Enum.GetNames(typeof(ShippingState)).Select(i => i.ToLower());

            var statuses = this._dataService.GetDbSet<Translation>()
                .Where(i => statusNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<ShippingState>())
                .ToList();

            var transportCompanies = _dataService.GetDbSet<TransportCompany>()
                                                 .Where(i => i.Title.ToLower().Contains(search)
                                                        && (i.CompanyId == null || companyId == null || i.CompanyId == companyId));

            var vehicleTypes = _dataService.GetDbSet<VehicleType>()
                                           .Where(i => i.Name.ToLower().Contains(search)
                                                    && (i.CompanyId == null || companyId == null || i.CompanyId == companyId));

            var shippingWarehouses = _dataService.GetDbSet<ShippingWarehouse>()
                                                 .Where(i => i.WarehouseName.ToLower().Contains(search)
                                                        && (i.CompanyId == null || companyId == null || i.CompanyId == companyId));

            var deliveryWarehouses = _dataService.GetDbSet<Warehouse>()
                                                 .Where(i => i.WarehouseName.ToLower().Contains(search)
                                                        && (i.CompanyId == null || companyId == null || i.CompanyId == companyId));

            return query.Where(i =>
               columns.Contains("shippingNumber") && !string.IsNullOrEmpty(i.ShippingNumber) && i.ShippingNumber.ToLower().Contains(search)
            || columns.Contains("shippingAddress") && !string.IsNullOrEmpty(i.ShippingAddress) && i.ShippingAddress.ToLower().Contains(search)
            || columns.Contains("deliveryAddress") && !string.IsNullOrEmpty(i.DeliveryAddress) && i.DeliveryAddress.ToLower().Contains(search)
            || columns.Contains("deliveryInvoiceNumber") && !string.IsNullOrEmpty(i.DeliveryInvoiceNumber) && i.DeliveryInvoiceNumber.ToLower().Contains(search)
            || columns.Contains("deviationReasonsComments") && !string.IsNullOrEmpty(i.DeviationReasonsComments) && i.DeviationReasonsComments.ToLower().Contains(search)
            || columns.Contains("additionalCostsComments") && !string.IsNullOrEmpty(i.AdditionalCostsComments) && i.AdditionalCostsComments.ToLower().Contains(search)
            || columns.Contains("costsComments") && !string.IsNullOrEmpty(i.CostsComments) && i.CostsComments.ToLower().Contains(search)
            || columns.Contains("invoiceNumber") && !string.IsNullOrEmpty(i.InvoiceNumber) && i.InvoiceNumber.ToLower().Contains(search)
            || columns.Contains("driverName") && !string.IsNullOrEmpty(i.DriverName) && i.DriverName.ToLower().Contains(search)
            || columns.Contains("driverPhone") && !string.IsNullOrEmpty(i.DriverPhone) && i.DriverPhone.ToLower().Contains(search)
            || columns.Contains("driverPassportData") && !string.IsNullOrEmpty(i.DriverPassportData) && i.DriverPassportData.ToLower().Contains(search)
            || columns.Contains("vehicleNumber") && !string.IsNullOrEmpty(i.VehicleNumber) && i.VehicleNumber.ToLower().Contains(search)
            || columns.Contains("vehicleMake") && !string.IsNullOrEmpty(i.VehicleMake) && i.VehicleMake.ToLower().Contains(search)
            || columns.Contains("trailerNumber") && !string.IsNullOrEmpty(i.TrailerNumber) && i.TrailerNumber.ToLower().Contains(search)
            || columns.Contains("routeNumber") && !string.IsNullOrEmpty(i.RouteNumber) && i.RouteNumber.ToLower().Contains(search)

            || columns.Contains("temperatureMin") && isInt && i.TemperatureMin == searchInt
            || columns.Contains("temperatureMax") && isInt && i.TemperatureMax == searchInt
            || columns.Contains("palletsCount") && isInt && i.PalletsCount == searchInt
            || columns.Contains("actualPalletsCount") && isInt && i.ActualPalletsCount == searchInt
            || columns.Contains("confirmedPalletsCount") && isInt && i.ConfirmedPalletsCount == searchInt
            || columns.Contains("bottlesCount") && isInt && i.BottlesCount == searchInt

            || columns.Contains("weightKg") && isDecimal && i.WeightKg != null && Math.Round(i.WeightKg.Value) >= searchDecimal - precision && Math.Round(i.WeightKg.Value) <= searchDecimal + precision
            || columns.Contains("actualWeightKg") && isDecimal && i.ActualWeightKg != null && Math.Round(i.ActualWeightKg.Value) >= searchDecimal - precision && Math.Round(i.ActualWeightKg.Value) <= searchDecimal + precision
            || columns.Contains("totalDeliveryCost") && isDecimal && i.TotalDeliveryCost >= searchDecimal - precision && i.TotalDeliveryCost <= searchDecimal + precision
            || columns.Contains("totalDeliveryCostWithoutVAT") && isDecimal && i.TotalDeliveryCostWithoutVAT >= searchDecimal - precision && i.TotalDeliveryCostWithoutVAT <= searchDecimal + precision
            || columns.Contains("basicDeliveryCostWithoutVAT") && isDecimal && i.BasicDeliveryCostWithoutVAT >= searchDecimal - precision && i.BasicDeliveryCostWithoutVAT <= searchDecimal + precision
            || columns.Contains("otherCosts") && isDecimal && i.OtherCosts >= searchDecimal - precision && i.OtherCosts <= searchDecimal + precision
            || columns.Contains("deliveryCostWithoutVAT") && isDecimal && i.DeliveryCostWithoutVAT >= searchDecimal - precision && i.DeliveryCostWithoutVAT <= searchDecimal + precision
            || columns.Contains("returnCostWithoutVAT") && isDecimal && i.ReturnCostWithoutVAT >= searchDecimal - precision && i.ReturnCostWithoutVAT <= searchDecimal + precision
            || columns.Contains("invoiceAmountWithoutVAT") && isDecimal && i.InvoiceAmountWithoutVAT >= searchDecimal - precision && i.InvoiceAmountWithoutVAT <= searchDecimal + precision
            || columns.Contains("additionalCostsWithoutVAT") && isDecimal && i.AdditionalCostsWithoutVAT >= searchDecimal - precision && i.AdditionalCostsWithoutVAT <= searchDecimal + precision
            || columns.Contains("extraPointCostsWithoutVAT") && isDecimal && i.ExtraPointCostsWithoutVAT >= searchDecimal - precision && i.ExtraPointCostsWithoutVAT <= searchDecimal + precision
            || columns.Contains("trucksDowntime") && isDecimal && i.TrucksDowntime >= searchDecimal - precision && i.TrucksDowntime <= searchDecimal + precision
            || columns.Contains("returnRate") && isDecimal && i.ReturnRate >= searchDecimal - precision && i.ReturnRate <= searchDecimal + precision
            || columns.Contains("additionalPointRate") && isDecimal && i.AdditionalPointRate >= searchDecimal - precision && i.AdditionalPointRate <= searchDecimal + precision
            || columns.Contains("downtimeRate") && isDecimal && i.DowntimeRate >= searchDecimal - precision && i.DowntimeRate <= searchDecimal + precision
            || columns.Contains("blankArrivalRate") && isDecimal && i.BlankArrivalRate >= searchDecimal - precision && i.BlankArrivalRate <= searchDecimal + precision
            || columns.Contains("loadingDowntimeCost") && isDecimal && i.LoadingDowntimeCost >= searchDecimal - precision && i.LoadingDowntimeCost <= searchDecimal + precision
            || columns.Contains("unloadingDowntimeCost") && isDecimal && i.UnloadingDowntimeCost >= searchDecimal - precision && i.UnloadingDowntimeCost <= searchDecimal + precision
            || columns.Contains("volume9l") && isDecimal && i.Volume9l >= searchDecimal - precision && i.Volume9l <= searchDecimal + precision

            || columns.Contains("shippingCreationDate") && i.ShippingCreationDate.Value.SqlFormat(searchDateFormat).Contains(search)
            || columns.Contains("loadingArrivalTime") && i.LoadingArrivalTime.Value.SqlFormat(searchDateFormat).Contains(search)
            || columns.Contains("loadingDepartureTime") && i.LoadingDepartureTime.Value.SqlFormat(searchDateFormat).Contains(search)
            || columns.Contains("shippingDate") && i.ShippingDate.Value.SqlFormat(searchDateFormat).Contains(search)
            || columns.Contains("deliveryDate") && i.DeliveryDate.Value.SqlFormat(searchDateFormat).Contains(search)
            || columns.Contains("documentsReturnDate") && i.DocumentsReturnDate.Value.SqlFormat(searchDateFormat).Contains(search)
            || columns.Contains("actualDocumentsReturnDate") && i.ActualDocumentsReturnDate.Value.SqlFormat(searchDateFormat).Contains(search)

            || columns.Contains("tarifficationType") && tarifficationTypes.Contains(i.TarifficationType)
            || columns.Contains("poolingProductType") && poolingProductTypes.Contains(i.PoolingProductType)
            || columns.Contains("deliveryType") && deliveryTypes.Contains(i.DeliveryType)
            || columns.Contains("vehicleTypeId") && vehicleTypes.Any(v => v.Id == i.VehicleTypeId)
            || columns.Contains("carrierId") && transportCompanies.Any(t => t.Id == i.CarrierId)
            || columns.Contains("shippingWarehouseId") && shippingWarehouses.Any(t => t.Id == i.ShippingWarehouseId)
            || columns.Contains("deliveryWarehouseId") && deliveryWarehouses.Any(t => t.Id == i.DeliveryWarehouseId)
            || columns.Contains("status") && statuses.Contains(i.Status)
            );
        }

        protected override ExcelMapper<ShippingDto> CreateExportExcelMapper()
        {
            string lang = _userIdProvider.GetCurrentUser()?.Language;
            return base.CreateExportExcelMapper()
                .MapColumn(w => w.CarrierId, new DictionaryReferenceExcelColumn<TransportCompany>(_dataService, _userIdProvider, x => x.Title))
                .MapColumn(w => w.BodyTypeId, new DictionaryReferenceExcelColumn<BodyType>(_dataService, _userIdProvider, x => x.Name))
                .MapColumn(w => w.VehicleTypeId, new DictionaryReferenceExcelColumn<VehicleType>(_dataService, _userIdProvider, x => x.Name))
                .MapColumn(w => w.ShippingWarehouseId, new DictionaryReferenceExcelColumn<ShippingWarehouse>(_dataService, _userIdProvider, x => x.WarehouseName))
                .MapColumn(w => w.DeliveryWarehouseId, new DictionaryReferenceExcelColumn<Warehouse>(_dataService, _userIdProvider, x => x.WarehouseName))
                .MapColumn(i => i.Status, new StateExcelColumn<ShippingState>(lang))
                .MapColumn(i => i.DeliveryType, new EnumExcelColumn<DeliveryType>(lang))
                .MapColumn(i => i.TarifficationType, new EnumExcelColumn<TarifficationType>(lang))
                .MapColumn(i => i.PoolingProductType, new EnumExcelColumn<PoolingProductType>(lang))
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userIdProvider, x => x.Name));
        }
    }
}