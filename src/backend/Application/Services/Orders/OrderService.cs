using Application.BusinessModels.Shared.Validation;
using Application.Extensions;
using Application.Shared;
using Application.Shared.Excel;
using Application.Shared.Excel.Columns;
using Application.Shared.Orders;
using Application.Shared.Triggers;
using AutoMapper;
using DAL.Extensions;
using DAL.Queries;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.FieldProperties;
using Domain.Services.History;
using Domain.Services.Orders;
using Domain.Shared;
using Domain.Shared.FormFilters;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Orders
{
    public class OrdersService : GridService<Order, OrderDto, OrderFormDto, OrderSummaryDto, OrderFilterDto>, IOrdersService
    {
        private readonly IHistoryService _historyService;
        private readonly IOrderChangesService _orderChangesService;

        public OrdersService(
            IHistoryService historyService,
            ICommonDataService dataService,
            IUserProvider userIdProvider,
            IFieldDispatcherService fieldDispatcherService,
            IFieldPropertiesService fieldPropertiesService,
            IServiceProvider serviceProvider,
            ITriggersService triggersService,
            IValidationService validationService,
            IOrderChangesService orderChangesService,
            IEnumerable<IValidationRule<OrderDto, Order>> validationRules)
            : base(dataService, userIdProvider, fieldDispatcherService, fieldPropertiesService, serviceProvider, triggersService, validationService, validationRules)
        {
            _mapper = ConfigureMapper().CreateMapper();
            _historyService = historyService;
            _orderChangesService = orderChangesService;
        }

        protected override IQueryable<Order> GetDbSet()
        {
            return base.GetDbSet()
                .Include(i => i.ShippingWarehouse)
                .Include(i => i.DeliveryWarehouse)
                .Include(i => i.Carrier)
                .Include(i => i.PickingType)
                .Include(i => i.VehicleType)
                .Include(i => i.BodyType)
                .Include(i => i.Company);
        }

        protected override void OnGetForm(Order entity, Role role)
        {
            bool hasChanges = _orderChangesService.ClearBacklightFlags(new[] { entity }, role);
            if (hasChanges)
            {
                _dataService.SaveChanges();
            }
        }

        public override OrderSummaryDto GetSummary(IEnumerable<Guid> ids)
        {
            var orders = this._dataService.GetDbSet<Order>().Where(o => ids.Contains(o.Id)).ToList();
            var result = new OrderSummaryDto
            {
                Count = orders.Count,
                BoxesCount = orders.Sum(o => o.BoxesCount ?? 0),
                PalletsCount = (int)Math.Ceiling(orders.Sum(o => o.PalletsCount ?? 0)),
                WeightKg = orders.Sum(o => o.WeightKg ?? 0M),
                DeliveryCost = orders.Sum(o => o.DeliveryCost ?? 0M),
                TrucksDowntime = orders.Sum(o => o.TrucksDowntime ?? 0M),
                DowntimeAmount = orders.Sum(o => o.DowntimeAmount ?? 0M),
                TotalAmount = orders.Sum(o => o.TotalAmount ?? 0M)
            };
            return result;
        }

        public IEnumerable<LookUpDto> FindByNumber(NumberSearchFormDto dto)
        {
            var companyId = _userIdProvider.GetCurrentUser()?.CompanyId;
            var dbSet = _dataService.GetDbSet<Order>();
            List<Order> entities;
            if (dto.IsPartial)
            {
                entities = dbSet.Where(x => x.OrderNumber.Contains(dto.Number, StringComparison.InvariantCultureIgnoreCase)
                                            && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                .ToList();
            }
            else
            {
                entities = dbSet.Where(x => x.OrderNumber == dto.Number
                                            && (x.CompanyId == null || companyId == null || x.CompanyId == companyId))
                                .ToList();
            }
            var result = entities.Select(MapFromEntityToLookupDto);
            return result;
        }

        protected override IQueryable<Order> ApplyRestrictions(IQueryable<Order> query)
        {
            query = base.ApplyRestrictions(query);

            var currentUserId = _userIdProvider.GetCurrentUserId();
            var user = _dataService.GetDbSet<User>().GetById(currentUserId.Value);

            if (user.CarrierId.HasValue)
            {
                var invalidStates = new[] { OrderState.Draft, OrderState.Created, OrderState.Confirmed, OrderState.Canceled };
                var invalidShippingStates = new[] { ShippingState.ShippingCreated, ShippingState.ShippingCanceled };

                query = query
                    .Where(x => x.CarrierId == user.CarrierId)
                    .Where(i => !invalidStates.Contains(i.Status))
                    .Where(i => !i.OrderShippingStatus.HasValue || !invalidShippingStates.Contains(i.OrderShippingStatus.Value));
            }

            return query;
        }

        public OrderFormDto GetFormByNumber(string orderNumber)
        {
            var entity = GetDbSet().Where(x => x.OrderNumber == orderNumber).FirstOrDefault();
            return MapFromEntityToFormDto(entity);
        }

        public override IEnumerable<EntityStatusDto<Order>> LoadStatusData(IEnumerable<Guid> ids)
        {
            var result = _dataService.GetDbSet<Order>()
                            .Where(x => ids.Contains(x.Id))
                            .Select(x => new EntityStatusDto<Order>
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
            var user = _userIdProvider.GetCurrentUser();
            var lang = user?.Language;

            var result = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OrderDto, OrderFormDto>();

                cfg.CreateMap<Order, OrderDto>()
                    .ForMember(t => t.Id, e => e.MapFrom((s, t) => s.Id.FormatGuid()))
                    .ForMember(t => t.Status, e => e.MapFrom((s, t) => s.Status.FormatEnum()))
                    .ForMember(t => t.OrderType, e => e.MapFrom((s, t) => s.OrderType == null ? null : s.OrderType.GetEnumLookup(lang)))
                    .ForMember(t => t.OrderDate, e => e.MapFrom((s, t) => s.OrderDate.FormatDate()))
                    .ForMember(t => t.ShippingStatus, e => e.MapFrom((s, t) => s.ShippingStatus.FormatEnum()))
                    .ForMember(t => t.DeliveryStatus, e => e.MapFrom((s, t) => s.DeliveryStatus.FormatEnum()))
                    .ForMember(t => t.OrderShippingStatus, e => e.MapFrom((s, t) => s.OrderShippingStatus.FormatEnum()))
                    .ForMember(t => t.PickingTypeId, e => e.MapFrom((s, t) => s.PickingType == null ? null : new LookUpDto(s.PickingTypeId.FormatGuid(), s.PickingType.ToString())))
                    .ForMember(t => t.ShippingWarehouseId, e => e.MapFrom((s, t) => s.ShippingWarehouse == null ? null : new LookUpDto(s.ShippingWarehouseId.FormatGuid(), s.ShippingWarehouse.ToString())))
                    .ForMember(t => t.DeliveryWarehouseId, e => e.MapFrom((s, t) => s.DeliveryWarehouse == null ? null : new LookUpDto(s.DeliveryWarehouseId.FormatGuid(), s.DeliveryWarehouse.ToString())))
                    .ForMember(t => t.ClientName, e => e.MapFrom((s, t) => string.IsNullOrEmpty(s.ClientName) ? null : new LookUpDto(s.ClientName)))
                    .ForMember(t => t.ShippingDate, e => e.MapFrom((s, t) => s.ShippingDate.FormatDateTime()))
                    .ForMember(t => t.DeliveryDate, e => e.MapFrom((s, t) => s.DeliveryDate.FormatDateTime()))
                    .ForMember(t => t.LoadingArrivalTime, e => e.MapFrom((s, t) => s.LoadingArrivalTime.FormatDateTime()))
                    .ForMember(t => t.LoadingDepartureTime, e => e.MapFrom((s, t) => s.LoadingDepartureTime.FormatDateTime()))
                    .ForMember(t => t.UnloadingArrivalTime, e => e.MapFrom((s, t) => s.UnloadingArrivalTime.FormatDateTime()))
                    .ForMember(t => t.UnloadingDepartureTime, e => e.MapFrom((s, t) => s.UnloadingDepartureTime.FormatDateTime()))
                    .ForMember(t => t.DocumentsReturnDate, e => e.MapFrom((s, t) => s.DocumentsReturnDate.FormatDate()))
                    .ForMember(t => t.ActualDocumentsReturnDate, e => e.MapFrom((s, t) => s.ActualDocumentsReturnDate.FormatDate()))
                    .ForMember(t => t.PlannedReturnDate, e => e.MapFrom((s, t) => s.PlannedReturnDate.FormatDate()))
                    .ForMember(t => t.ActualReturnDate, e => e.MapFrom((s, t) => s.ActualReturnDate.FormatDate()))
                    .ForMember(t => t.CarrierId, e => e.MapFrom((s, t) => s.Carrier == null ? null : new LookUpDto(s.CarrierId.FormatGuid(), s.Carrier.ToString())))
                    .ForMember(t => t.BodyTypeId, e => e.MapFrom((s, t) => s.BodyType == null ? null : new LookUpDto(s.BodyTypeId.FormatGuid(), s.BodyType.ToString())))
                    .ForMember(t => t.DeliveryType, e => e.MapFrom((s, t) => s.DeliveryType == null ? null : s.DeliveryType.GetEnumLookup(lang)))
                    .ForMember(t => t.VehicleTypeId, e => e.MapFrom((s, t) => s.VehicleType == null ? null : new LookUpDto(s.VehicleTypeId.FormatGuid(), s.VehicleType.ToString())))
                    .ForMember(t => t.CompanyId, e => e.MapFrom((s, t) => s.CompanyId == null ? null : new LookUpDto(s.CompanyId.FormatGuid(), s.Company.ToString())))
                    .ForMember(t => t.TarifficationType, e => e.MapFrom((s, t) => s.TarifficationType == null ? null : s.TarifficationType.GetEnumLookup(lang)))
                    .ForMember(t => t.ShippingWarehouseState, e => e.MapFrom((s, t) => s.ShippingWarehouseState.GetEnumLookup(lang)));

                cfg.CreateMap<OrderItem, OrderItemDto>()
                    .ForMember(t => t.Id, e => e.MapFrom((s, t) => s.Id.FormatGuid()));
            });
            return result;
        }

        public override void MapFromDtoToEntity(Order entity, OrderDto dto)
        {
            _orderChangesService.MapFromDtoToEntity(entity, dto);
        }

        public override void MapFromFormDtoToEntity(Order entity, OrderFormDto dto)
        {
            dto.ArticlesCount = (dto.Items?.Count).GetValueOrDefault();

            MapFromDtoToEntity(entity, dto);

            SaveItems(entity, dto);
        }

        public override OrderDto MapFromEntityToDto(Order entity, Role role)
        {
            if (entity == null)
            {
                return null;
            }

            var dto = _mapper.Map<OrderDto>(entity);

            return dto;
        }

        public override OrderFormDto MapFromEntityToFormDto(Order entity)
        {
            if (entity == null)
            {
                return null;
            }

            OrderDto dto = _mapper.Map<OrderDto>(entity);
            OrderFormDto result = _mapper.Map<OrderFormDto>(dto);

            result.Items = _dataService.GetDbSet<OrderItem>()
                                      .Where(i => i.OrderId == entity.Id)
                                      .Select(_mapper.Map<OrderItemDto>)
                                      .ToList();

            return result;
        }

        public override LookUpDto MapFromEntityToLookupDto(Order entity)
        {
            return new LookUpDto
            {
                Value = entity.Id.FormatGuid(),
                Name = entity.OrderNumber
            };
        }

        private void SaveItems(Order entity, OrderFormDto dto)
        {
            bool isManual = !(dto.AdditionalInfo?.Contains("INJECTION") ?? false);
            if (dto.Items != null)
            {
                HashSet<Guid> updatedItems = new HashSet<Guid>();
                List<OrderItem> entityItems = _dataService.GetDbSet<OrderItem>().Where(i => i.OrderId == entity.Id).ToList();
                Dictionary<string, OrderItem> entityItemsDict = entityItems.ToDictionary(i => i.Id.FormatGuid());
                foreach (OrderItemDto itemDto in dto.Items)
                {
                    OrderItem item;
                    if (string.IsNullOrEmpty(itemDto.Id) || !entityItemsDict.TryGetValue(itemDto.Id, out item))
                    {
                        item = new OrderItem
                        {
                            OrderId = entity.Id
                        };
                        MapFromItemDtoToEntity(item, itemDto, entity.CompanyId, true, isManual);

                        _historyService.Save(entity.Id, "orderItemAdded", item.Nart, item.Quantity);
                    }
                    else
                    {
                        updatedItems.Add(item.Id);
                        MapFromItemDtoToEntity(item, itemDto, entity.CompanyId, false, isManual);
                        //_dataService.GetDbSet<OrderItem>().Update(item);
                    }
                }

                var itemsToRemove = entityItems.Where(i => !updatedItems.Contains(i.Id) && (isManual || !i.IsManualEdited));
                foreach (var item in itemsToRemove)
                {
                    _historyService.Save(entity.Id, "orderItemRemoved", item.Nart, item.Quantity);
                }
                _dataService.GetDbSet<OrderItem>().RemoveRange(itemsToRemove);

                entity.ArticlesCount = dto.Items.Count;
            }
        }

        private void MapFromItemDtoToEntity(OrderItem entity, OrderItemDto dto, Guid? companyId, bool isNew, bool isManual)
        {
            if (!string.IsNullOrEmpty(dto.Id))
                entity.Id = dto.Id.ToGuid().Value;

            entity.Nart = dto.Nart;
            entity.Quantity = dto.Quantity ?? 0;

            if (isNew)
            {
                _dataService.GetDbSet<OrderItem>().Add(entity);
            }

            var change = _dataService.GetChanges<OrderItem>().FirstOrDefault(x => x.Entity.Id == entity.Id);
            if (isManual && change.FieldChanges.Any())
            {
                entity.IsManualEdited = true;
            }
        }

        private readonly IMapper _mapper;

        protected override IQueryable<Order> ApplySort(IQueryable<Order> query, FilterFormDto<OrderFilterDto> searchForm)
        {
            var sortFieldMapping = new Dictionary<string, string>
            {
                { "unloadingArrivalDate", "unloadingArrivalTime" },
                { "unloadingDepartureDate", "unloadingDepartureTime" }
            };

            if (!string.IsNullOrEmpty(searchForm.Sort?.Name) && sortFieldMapping.ContainsKey(searchForm.Sort?.Name))
            {
                searchForm.Sort.Name = sortFieldMapping[searchForm.Sort?.Name];
            }

            return query.OrderBy(searchForm.Sort?.Name, searchForm.Sort?.Desc == true)
                .DefaultOrderBy(i => i.OrderCreationDate, !string.IsNullOrEmpty(searchForm.Sort?.Name), true)
                .DefaultOrderBy(i => i.Id, true);
        }

        /// <summary>
        /// Apply search form filter to query
        /// </summary>
        /// <param name="query">query</param>
        /// <param name="searchForm">search form</param>
        /// <returns></returns>
        public override IQueryable<Order> ApplySearchForm(IQueryable<Order> query, FilterFormDto<OrderFilterDto> searchForm, List<string> columns = null)
        {
            List<object> parameters = new List<object>();
            string where = string.Empty;

            // OrderNumber Filter
            where = where.WhereAnd(searchForm.Filter.ActualPalletsCount.ApplyNumericFilter<Order>(i => i.ActualPalletsCount, ref parameters))
                         .WhereAnd(searchForm.Filter.ActualReturnDate.ApplyDateRangeFilter<Order>(i => i.ActualReturnDate, ref parameters))
                         .WhereAnd(searchForm.Filter.ActualWeightKg.ApplyNumericFilter<Order>(i => i.ActualWeightKg, ref parameters))
                         .WhereAnd(searchForm.Filter.Volume.ApplyNumericFilter<Order>(i => i.Volume, ref parameters))
                         .WhereAnd(searchForm.Filter.ArticlesCount.ApplyNumericFilter<Order>(i => i.ArticlesCount, ref parameters))
                         .WhereAnd(searchForm.Filter.ClientOrderNumber.ApplyStringFilter<Order>(i => i.ClientOrderNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.BodyTypeId.ApplyOptionsFilter<Order, Guid?>(i => i.BodyTypeId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.BoxesCount.ApplyNumericFilter<Order>(i => i.BoxesCount, ref parameters))
                         .WhereAnd(searchForm.Filter.ClientName.ApplyOptionsFilter<Order, string>(i => i.ClientName, ref parameters))
                         .WhereAnd(searchForm.Filter.ConfirmedBoxesCount.ApplyNumericFilter<Order>(i => i.ConfirmedBoxesCount, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryAddress.ApplyStringFilter<Order>(i => i.DeliveryAddress, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryCity.ApplyOptionsFilter<Order, string>(i => i.DeliveryCity, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryDate.ApplyDateRangeFilter<Order>(i => i.DeliveryDate, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryRegion.ApplyOptionsFilter<Order, string>(i => i.DeliveryRegion, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryStatus.ApplyEnumFilter<Order, VehicleState>(i => i.DeliveryStatus, ref parameters))
                         .WhereAnd(searchForm.Filter.Invoice.ApplyBooleanFilter<Order>(i => i.Invoice, ref parameters))
                         .WhereAnd(searchForm.Filter.LoadingArrivalTime.ApplyDateRangeFilter<Order>(i => i.LoadingArrivalTime, ref parameters))
                         .WhereAnd(searchForm.Filter.LoadingDepartureTime.ApplyDateRangeFilter<Order>(i => i.LoadingDepartureTime, ref parameters))
                         .WhereAnd(searchForm.Filter.MajorAdoptionNumber.ApplyStringFilter<Order>(i => i.MajorAdoptionNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.OrderAmountExcludingVAT.ApplyNumericFilter<Order>(i => i.OrderAmountExcludingVAT, ref parameters))
                         .WhereAnd(searchForm.Filter.OrderChangeDate.ApplyDateRangeFilter<Order>(i => i.OrderChangeDate, ref parameters))
                         .WhereAnd(searchForm.Filter.OrderComments.ApplyStringFilter<Order>(i => i.OrderComments, ref parameters))
                         .WhereAnd(searchForm.Filter.OrderCreationDate.ApplyDateRangeFilter<Order>(i => i.OrderCreationDate, ref parameters))
                         .WhereAnd(searchForm.Filter.OrderDate.ApplyDateRangeFilter<Order>(i => i.OrderDate, ref parameters))
                         .WhereAnd(searchForm.Filter.OrderNumber.ApplyStringFilter<Order>(i => i.OrderNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.OrderShippingStatus.ApplyEnumFilter<Order, ShippingState>(i => i.OrderShippingStatus, ref parameters))
                         .WhereAnd(searchForm.Filter.OrderType.ApplyEnumFilter<Order, OrderType>(i => i.OrderType, ref parameters))
                         .WhereAnd(searchForm.Filter.PalletsCount.ApplyNumericFilter<Order>(i => i.PalletsCount, ref parameters))
                         .WhereAnd(searchForm.Filter.Payer.ApplyStringFilter<Order>(i => i.Payer, ref parameters))
                         .WhereAnd(searchForm.Filter.PickingTypeId.ApplyOptionsFilter<Order, Guid?>(i => i.PickingTypeId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.PlannedReturnDate.ApplyDateRangeFilter<Order>(i => i.PlannedReturnDate, ref parameters))
                         .WhereAnd(searchForm.Filter.ReturnInformation.ApplyStringFilter<Order>(i => i.ReturnInformation, ref parameters))
                         .WhereAnd(searchForm.Filter.ReturnShippingAccountNo.ApplyStringFilter<Order>(i => i.ReturnShippingAccountNo, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingAddress.ApplyStringFilter<Order>(i => i.ShippingAddress, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingCity.ApplyOptionsFilter<Order, string>(i => i.ShippingCity, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingDate.ApplyDateRangeFilter<Order>(i => i.ShippingDate, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingId.ApplyOptionsFilter<Order, Guid?>(i => i.ShippingId, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingNumber.ApplyStringFilter<Order>(i => i.ShippingNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingRegion.ApplyStringFilter<Order>(i => i.ShippingRegion, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingStatus.ApplyEnumFilter<Order, VehicleState>(i => i.ShippingStatus, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingWarehouseId.ApplyOptionsFilter<Order, Guid?>(i => i.ShippingWarehouseId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.DeliveryWarehouseId.ApplyOptionsFilter<Order, Guid?>(i => i.DeliveryWarehouseId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.CompanyId.ApplyOptionsFilter<Order, Guid?>(i => i.CompanyId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.SoldTo.ApplyOptionsFilter<Order, string>(i => i.SoldTo, ref parameters))
                         .WhereAnd(searchForm.Filter.Status.ApplyEnumFilter<Order, OrderState>(i => i.Status, ref parameters))
                         .WhereAnd(searchForm.Filter.TemperatureMax.ApplyNumericFilter<Order>(i => i.TemperatureMax, ref parameters))
                         .WhereAnd(searchForm.Filter.TemperatureMin.ApplyNumericFilter<Order>(i => i.TemperatureMin, ref parameters))
                         .WhereAnd(searchForm.Filter.ConfirmedPalletsCount.ApplyNumericFilter<Order>(i => i.ConfirmedPalletsCount, ref parameters))
                         .WhereAnd(searchForm.Filter.TransitDays.ApplyNumericFilter<Order>(i => i.TransitDays, ref parameters))
                         .WhereAnd(searchForm.Filter.TrucksDowntime.ApplyNumericFilter<Order>(i => i.TrucksDowntime, ref parameters))
                         .WhereAnd(searchForm.Filter.UnloadingArrivalTime.ApplyDateRangeFilter<Order>(i => i.UnloadingArrivalTime, ref parameters))
                         .WhereAnd(searchForm.Filter.UnloadingDepartureTime.ApplyDateRangeFilter<Order>(i => i.UnloadingDepartureTime, ref parameters))
                         .WhereAnd(searchForm.Filter.DocumentsReturnDate.ApplyDateRangeFilter<Order>(i => i.DocumentsReturnDate, ref parameters))
                         .WhereAnd(searchForm.Filter.ActualDocumentsReturnDate.ApplyDateRangeFilter<Order>(i => i.ActualDocumentsReturnDate, ref parameters))
                         .WhereAnd(searchForm.Filter.WeightKg.ApplyNumericFilter<Order>(i => i.WeightKg, ref parameters))
                         .WhereAnd(searchForm.Filter.WaybillTorg12.ApplyBooleanFilter<Order>(i => i.WaybillTorg12, ref parameters))
                         .WhereAnd(searchForm.Filter.PickingFeatures.ApplyStringFilter<Order>(i => i.PickingFeatures, ref parameters))
                         .WhereAnd(searchForm.Filter.CarrierId.ApplyOptionsFilter<Order, Guid?>(i => i.CarrierId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.VehicleTypeId.ApplyOptionsFilter<Order, Guid?>(i => i.VehicleTypeId, ref parameters, i => new Guid(i)))
                         .WhereAnd(searchForm.Filter.DeliveryType.ApplyEnumFilter<Order, DeliveryType>(i => i.DeliveryType, ref parameters))
                         .WhereAnd(searchForm.Filter.TarifficationType.ApplyEnumFilter<Order, TarifficationType>(i => i.TarifficationType, ref parameters))
                         .WhereAnd(searchForm.Filter.ShippingWarehouseState.ApplyEnumFilter<Order, WarehouseOrderState>(i => i.ShippingWarehouseState, ref parameters))
                         .WhereAnd(searchForm.Filter.DeviationsComment.ApplyStringFilter<Order>(i => i.DeviationsComment, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryCost.ApplyNumericFilter<Order>(i => i.DeliveryCost, ref parameters))
                         .WhereAnd(searchForm.Filter.BookingNumber.ApplyStringFilter<Order>(i => i.BookingNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.DowntimeAmount.ApplyNumericFilter<Order>(i => i.DowntimeAmount, ref parameters))
                         .WhereAnd(searchForm.Filter.OtherExpenses.ApplyNumericFilter<Order>(i => i.OtherExpenses, ref parameters))
                         .WhereAnd(searchForm.Filter.TotalAmount.ApplyNumericFilter<Order>(i => i.TotalAmount, ref parameters))
                         .WhereAnd(searchForm.Filter.TotalAmountNds.ApplyNumericFilter<Order>(i => i.TotalAmountNds, ref parameters))
                         .WhereAnd(searchForm.Filter.ReturnShippingCost.ApplyNumericFilter<Order>(i => i.ReturnShippingCost, ref parameters))
                         .WhereAnd(searchForm.Filter.DeliveryAccountNumber.ApplyStringFilter<Order>(i => i.DeliveryAccountNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.DocumentAttached.ApplyBooleanFilter<Order>(i => i.DocumentAttached, ref parameters))
                         .WhereAnd(searchForm.Filter.AmountConfirmed.ApplyBooleanFilter<Order>(i => i.AmountConfirmed, ref parameters))
                         .WhereAnd(searchForm.Filter.DocumentReturnStatus.ApplyBooleanFilter<Order>(i => i.DocumentReturnStatus, ref parameters))
                         .WhereAnd(searchForm.Filter.IsPooling.ApplyBooleanFilter<Order>(i => i.IsPooling, ref parameters))
                         .WhereAnd(searchForm.Filter.IsReturn.ApplyBooleanFilter<Order>(i => i.IsReturn, ref parameters))
                         .WhereAnd(searchForm.Filter.DriverPassportData.ApplyStringFilter<Order>(i => i.DriverPassportData, ref parameters))
                         .WhereAnd(searchForm.Filter.VehicleMake.ApplyStringFilter<Order>(i => i.VehicleMake, ref parameters))
                         .WhereAnd(searchForm.Filter.TrailerNumber.ApplyStringFilter<Order>(i => i.TrailerNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.DriverName.ApplyStringFilter<Order>(i => i.DriverName, ref parameters))
                         .WhereAnd(searchForm.Filter.DriverPhone.ApplyStringFilter<Order>(i => i.DriverPhone, ref parameters))
                         .WhereAnd(searchForm.Filter.VehicleNumber.ApplyStringFilter<Order>(i => i.VehicleNumber, ref parameters))
                         .WhereAnd(searchForm.Filter.TransportZone.ApplyStringFilter<Order>(i => i.TransportZone, ref parameters))
                         .WhereAnd(searchForm.Filter.BottlesCount.ApplyNumericFilter<Order>(i => i.BottlesCount, ref parameters))
                         .WhereAnd(searchForm.Filter.Volume9l.ApplyNumericFilter<Order>(i => i.Volume9l, ref parameters))
                         .WhereAnd(searchForm.Filter.PaymentCondition.ApplyStringFilter<Order>(i => i.PaymentCondition, ref parameters));

            string sql = $@"SELECT * FROM ""Orders"" {where}";
            query = query.FromSql(sql, parameters.ToArray());

            // Apply Search
            return this.ApplySearch(query, searchForm?.Filter?.Search, columns ?? searchForm?.Filter?.Columns);
        }

        private IQueryable<Order> ApplySearch(IQueryable<Order> query, string search, List<string> columns)
        {
            if (string.IsNullOrEmpty(search)) return query;

            search = search.ToLower().Trim();

            var isInt = int.TryParse(search, out int searchInt);

            decimal? searchDecimal = search.ToDecimal();
            var isDecimal = searchDecimal != null;
            decimal precision = 0.01M;

            var companyId = _userIdProvider.GetCurrentUser()?.CompanyId;

            var pickingTypes = _dataService.GetDbSet<PickingType>()
                                           .Where(i => i.Name.ToLower().Contains(search)
                                                    && (i.CompanyId == null || companyId == null || i.CompanyId == companyId));

            var carriers = _dataService.GetDbSet<TransportCompany>()
                                       .Where(i => i.Title.ToLower().Contains(search)
                                                && (i.CompanyId == null || companyId == null || i.CompanyId == companyId));

            var orderTypeNames = Enum.GetNames(typeof(OrderType)).Select(i => i.ToLower());

            var orderTypes = _dataService.GetDbSet<Translation>()
                .Where(i => orderTypeNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<OrderType>())
                .ToList();

            var orderShippingStateNames = Enum.GetNames(typeof(ShippingState)).Select(i => i.ToLower());

            var orderShippingStates = _dataService.GetDbSet<Translation>()
                .Where(i => orderShippingStateNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<ShippingState>())
                .ToList();

            var orderStateNames = Enum.GetNames(typeof(OrderState)).Select(i => i.ToLower());

            var orderStates = _dataService.GetDbSet<Translation>()
                .Where(i => orderStateNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<OrderState>())
                .ToList();

            var vehicleStateNames = Enum.GetNames(typeof(VehicleState)).Select(i => i.ToLower());

            var vehicleStates = _dataService.GetDbSet<Translation>()
                .Where(i => vehicleStateNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<VehicleState>())
                .ToList();

            var deliveryTypeNames = Enum.GetNames(typeof(DeliveryType)).Select(i => i.ToLower());

            var deliveryTypes = _dataService.GetDbSet<Translation>()
                .Where(i => deliveryTypeNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<DeliveryType>())
                .ToList();

            var shippingWarehouseStateNames = Enum.GetNames(typeof(WarehouseOrderState)).Select(i => i.ToLower());

            var shippingWarehouseStates = _dataService.GetDbSet<Translation>()
                .Where(i => shippingWarehouseStateNames.Contains(i.Name.ToLower()))
                .WhereTranslation(search)
                .Select(i => i.Name.ToEnum<WarehouseOrderState>())
                .ToList();

            return query.Where(i =>
                   columns.Contains("orderNumber") && !string.IsNullOrEmpty(i.OrderNumber) && i.OrderNumber.ToLower().Contains(search)
                || columns.Contains("shippingNumber") && !string.IsNullOrEmpty(i.ShippingNumber) && i.ShippingNumber.ToLower().Contains(search)
                || columns.Contains("clientName") && !string.IsNullOrEmpty(i.ClientName) && i.ClientName.ToLower().Contains(search)
                || columns.Contains("soldTo") && !string.IsNullOrEmpty(i.SoldTo) && i.SoldTo.ToLower().Contains(search)
                || columns.Contains("payer") && !string.IsNullOrEmpty(i.Payer) && i.Payer.ToLower().Contains(search)
                || columns.Contains("shippingAddress") && !string.IsNullOrEmpty(i.ShippingAddress) && i.ShippingAddress.ToLower().Contains(search)
                || columns.Contains("shippingCity") && !string.IsNullOrEmpty(i.ShippingCity) && i.ShippingCity.ToLower().Contains(search)
                || columns.Contains("shippingRegion") && !string.IsNullOrEmpty(i.ShippingRegion) && i.ShippingRegion.ToLower().Contains(search)
                || columns.Contains("deliveryRegion") && !string.IsNullOrEmpty(i.DeliveryRegion) && i.DeliveryRegion.ToLower().Contains(search)
                || columns.Contains("deliveryCity") && !string.IsNullOrEmpty(i.DeliveryCity) && i.DeliveryCity.ToLower().Contains(search)
                || columns.Contains("deliveryAddress") && !string.IsNullOrEmpty(i.DeliveryAddress) && i.DeliveryAddress.ToLower().Contains(search)
                || columns.Contains("clientOrderNumber") && !string.IsNullOrEmpty(i.ClientOrderNumber) && i.ClientOrderNumber.ToLower().Contains(search)
                || columns.Contains("returnInformation") && !string.IsNullOrEmpty(i.ReturnInformation) && i.ReturnInformation.ToLower().Contains(search)
                || columns.Contains("returnShippingAccountNo") && !string.IsNullOrEmpty(i.ReturnShippingAccountNo) && i.ReturnShippingAccountNo.ToLower().Contains(search)
                || columns.Contains("majorAdoptionNumber") && !string.IsNullOrEmpty(i.MajorAdoptionNumber) && i.MajorAdoptionNumber.ToLower().Contains(search)
                || columns.Contains("orderComments") && !string.IsNullOrEmpty(i.OrderComments) && i.OrderComments.ToLower().Contains(search)
                || columns.Contains("deviationsComment") && !string.IsNullOrEmpty(i.DeviationsComment) && i.DeviationsComment.ToLower().Contains(search)
                || columns.Contains("deliveryAccountNumber") && !string.IsNullOrEmpty(i.DeliveryAccountNumber) && i.DeliveryAccountNumber.ToLower().Contains(search)
                || columns.Contains("driverName") && !string.IsNullOrEmpty(i.DriverName) && i.DriverName.ToLower().Contains(search)
                || columns.Contains("driverPhone") && !string.IsNullOrEmpty(i.DriverPhone) && i.DriverPhone.ToLower().Contains(search)
                || columns.Contains("vehicleNumber") && !string.IsNullOrEmpty(i.VehicleNumber) && i.VehicleNumber.ToLower().Contains(search)
                || columns.Contains("driverPassportData") && !string.IsNullOrEmpty(i.DriverPassportData) && i.DriverPassportData.ToLower().Contains(search)
                || columns.Contains("vehicleMake") && !string.IsNullOrEmpty(i.VehicleMake) && i.VehicleMake.ToLower().Contains(search)
                || columns.Contains("trailerNumber") && !string.IsNullOrEmpty(i.TrailerNumber) && i.TrailerNumber.ToLower().Contains(search)
                || columns.Contains("transportZone") && !string.IsNullOrEmpty(i.TransportZone) && i.TransportZone.ToLower().Contains(search)
                || columns.Contains("paymentCondition") && !string.IsNullOrEmpty(i.PaymentCondition) && i.PaymentCondition.ToLower().Contains(search)

                || columns.Contains("temperatureMin") && isInt && i.TemperatureMin == searchInt
                || columns.Contains("temperatureMax") && isInt && i.TemperatureMax == searchInt
                || columns.Contains("transitDays") && isInt && i.TransitDays == searchInt
                || columns.Contains("articlesCount") && isInt && i.ArticlesCount == searchInt
                || columns.Contains("bottlesCount") && isInt && i.BottlesCount == searchInt

                || columns.Contains("orderDate") && i.OrderDate.HasValue && i.OrderDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("shippingDate") && i.ShippingDate.HasValue && i.ShippingDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("deliveryDate") && i.DeliveryDate.HasValue && i.DeliveryDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("loadingArrivalTime") && i.LoadingArrivalTime.HasValue && i.LoadingArrivalTime.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("loadingDepartureTime") && i.LoadingDepartureTime.HasValue && i.LoadingDepartureTime.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("unloadingArrivalTime") && i.UnloadingArrivalTime.HasValue && i.UnloadingArrivalTime.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("unloadingDepartureTime") && i.UnloadingDepartureTime.HasValue && i.UnloadingDepartureTime.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("plannedReturnDate") && i.PlannedReturnDate.HasValue && i.PlannedReturnDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("actualReturnDate") && i.ActualReturnDate.HasValue && i.ActualReturnDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("actualDocumentsReturnDate") && i.ActualDocumentsReturnDate.HasValue && i.ActualDocumentsReturnDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("orderCreationDate") && i.OrderCreationDate.HasValue && i.OrderCreationDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)
                || columns.Contains("orderChangeDate") && i.OrderChangeDate.HasValue && i.OrderChangeDate.Value.SqlFormat(StringExt.SqlDateFormat).Contains(search)

                || columns.Contains("boxesCount") && isDecimal && i.BoxesCount >= searchDecimal - precision && i.BoxesCount <= searchDecimal + precision
                || columns.Contains("confirmedBoxesCount") && isDecimal && i.ConfirmedBoxesCount >= searchDecimal - precision && i.ConfirmedBoxesCount <= searchDecimal + precision
                || columns.Contains("confirmedPalletsCount") && isDecimal && i.ConfirmedPalletsCount >= searchDecimal - precision && i.ConfirmedPalletsCount <= searchDecimal + precision
                || columns.Contains("palletsCount") && isDecimal && i.PalletsCount >= searchDecimal - precision && i.PalletsCount <= searchDecimal + precision
                || columns.Contains("actualPalletsCount") && isDecimal && i.ActualPalletsCount >= searchDecimal - precision && i.ActualPalletsCount <= searchDecimal + precision
                || columns.Contains("weightKg") && isDecimal && i.WeightKg != null && Math.Round(i.WeightKg.Value) >= searchDecimal - precision && Math.Round(i.WeightKg.Value) <= searchDecimal + precision
                || columns.Contains("actualWeightKg") && isDecimal && i.ActualWeightKg != null && Math.Round(i.ActualWeightKg.Value) >= searchDecimal - precision && Math.Round(i.ActualWeightKg.Value) <= searchDecimal + precision
                || columns.Contains("volume") && isDecimal && i.Volume != null && Math.Round(i.Volume.Value) >= searchDecimal - precision && Math.Round(i.Volume.Value) <= searchDecimal + precision
                || columns.Contains("orderAmountExcludingVAT") && isDecimal && i.OrderAmountExcludingVAT >= searchDecimal - precision && i.OrderAmountExcludingVAT <= searchDecimal + precision
                || columns.Contains("trucksDowntime") && isDecimal && i.TrucksDowntime >= searchDecimal - precision && i.TrucksDowntime <= searchDecimal + precision
                || columns.Contains("deliveryCost") && isDecimal && i.DeliveryCost >= searchDecimal - precision && i.DeliveryCost <= searchDecimal + precision
                || columns.Contains("downtimeAmount") && isDecimal && i.DowntimeAmount >= searchDecimal - precision && i.DowntimeAmount <= searchDecimal + precision
                || columns.Contains("otherExpenses") && isDecimal && i.OtherExpenses >= searchDecimal - precision && i.OtherExpenses <= searchDecimal + precision
                || columns.Contains("totalAmount") && isDecimal && i.TotalAmount >= searchDecimal - precision && i.TotalAmount <= searchDecimal + precision
                || columns.Contains("totalAmountNds") && isDecimal && i.TotalAmountNds >= searchDecimal - precision && i.TotalAmountNds <= searchDecimal + precision
                || columns.Contains("returnShippingCost") && isDecimal && i.ReturnShippingCost >= searchDecimal - precision && i.ReturnShippingCost <= searchDecimal + precision
                || columns.Contains("volume9l") && isDecimal && i.Volume9l >= searchDecimal - precision && i.Volume9l <= searchDecimal + precision

                || columns.Contains("pickingTypeId") && pickingTypes.Any(p => p.Id == i.PickingTypeId)
                || columns.Contains("carrierId") && carriers.Any(p => p.Id == i.CarrierId)
                || columns.Contains("orderType") && orderTypes.Contains(i.OrderType)
                || columns.Contains("orderShippingStatus") && orderShippingStates.Contains(i.OrderShippingStatus)
                || columns.Contains("deliveryStatus") && vehicleStates.Contains(i.DeliveryStatus)
                || columns.Contains("shippingStatus") && vehicleStates.Contains(i.ShippingStatus)
                || columns.Contains("status") && orderStates.Contains(i.Status)
                || columns.Contains("deliveryType") && deliveryTypes.Contains(i.DeliveryType)
                || columns.Contains("shippingWarehouseState") && shippingWarehouseStates.Contains(i.ShippingWarehouseState)
                );
        }

        protected override ExcelMapper<OrderDto> CreateExportExcelMapper()
        {
            string lang = _userIdProvider.GetCurrentUser()?.Language;
            return base.CreateExportExcelMapper()
                .MapColumn(i => i.PickingTypeId, new DictionaryReferenceExcelColumn<PickingType>(_dataService, _userIdProvider, x => x.Name))
                .MapColumn(i => i.ShippingWarehouseId, new DictionaryReferenceExcelColumn<ShippingWarehouse>(_dataService, _userIdProvider, x => x.WarehouseName))
                .MapColumn(i => i.DeliveryWarehouseId, new DictionaryReferenceExcelColumn<Warehouse>(_dataService, _userIdProvider, x => x.WarehouseName))
                .MapColumn(i => i.CarrierId, new DictionaryReferenceExcelColumn<TransportCompany>(_dataService, _userIdProvider, x => x.Title))
                .MapColumn(i => i.Status, new StateExcelColumn<OrderState>(lang))
                .MapColumn(i => i.OrderShippingStatus, new StateExcelColumn<ShippingState>(lang))
                .MapColumn(i => i.ShippingStatus, new StateExcelColumn<VehicleState>(lang))
                .MapColumn(i => i.DeliveryStatus, new StateExcelColumn<VehicleState>(lang))
                .MapColumn(i => i.OrderType, new EnumExcelColumn<OrderType>(lang))
                .MapColumn(i => i.DeliveryType, new EnumExcelColumn<DeliveryType>(lang))
                .MapColumn(i => i.TarifficationType, new EnumExcelColumn<TarifficationType>(lang))
                .MapColumn(i => i.ShippingWarehouseState, new EnumExcelColumn<WarehouseOrderState>(lang))
                .MapColumn(w => w.BodyTypeId, new DictionaryReferenceExcelColumn<BodyType>(_dataService, _userIdProvider, x => x.Name))
                .MapColumn(w => w.VehicleTypeId, new DictionaryReferenceExcelColumn<VehicleType>(_dataService, _userIdProvider, x => x.Name))
                .MapColumn(w => w.CompanyId, new DictionaryReferenceExcelColumn<Company>(_dataService, _userIdProvider, x => x.Name));
        }
    }
}