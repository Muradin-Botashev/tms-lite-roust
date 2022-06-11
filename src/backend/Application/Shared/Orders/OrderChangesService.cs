using AutoMapper;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.History;
using Domain.Services.Orders;
using Domain.Shared.UserProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Shared.Orders
{
    public class OrderChangesService : IOrderChangesService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly IHistoryService _historyService;
        private readonly IMapper _mapper;

        public OrderChangesService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            IHistoryService historyService)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _historyService = historyService;
            _mapper = ConfigureMapper().CreateMapper();
        }

        public bool ClearBacklightFlags(IEnumerable<Order> entities, Role role)
        {
            bool result = false;

            if (role?.Backlights != null
                && role.Backlights.Contains((int)BacklightType.OrderConfirmedBacklight))
            {
                var entitiesToUpdate = entities.Where(x => x.IsNewForConfirmed);
                if (entitiesToUpdate.Any())
                {
                    foreach (var entity in entitiesToUpdate)
                    {
                        entity.IsNewForConfirmed = false;

                        _historyService.Save(entity.Id, "orderSetGetToWork", entity.OrderNumber);
                    }

                    result = true;
                }
            }

            if (role?.Backlights != null
                && role.Backlights.Contains((int)BacklightType.CarrierRequestSentBacklight))
            {
                var entitiesToUpdate = entities.Where(x => x.IsNewCarrierRequest);
                if (entitiesToUpdate.Any())
                {
                    foreach (var entity in entitiesToUpdate)
                    {
                        entity.IsNewCarrierRequest = false;
                    }

                    var orderIds = entitiesToUpdate.Select(x => x.Id).ToList();
                    var shippingIds = entitiesToUpdate.Where(x => x.ShippingId.HasValue)
                                                      .Select(x => x.ShippingId.Value)
                                                      .Distinct()
                                                      .ToList();

                    var activeShippingIds = _dataService.GetDbSet<Order>()
                                                        .Where(x => x.ShippingId != null
                                                                && shippingIds.Contains(x.ShippingId.Value)
                                                                && !orderIds.Contains(x.Id)
                                                                && x.IsNewCarrierRequest)
                                                        .Select(x => x.ShippingId.Value)
                                                        .Distinct()
                                                        .ToHashSet();

                    foreach (var shippingId in shippingIds)
                    {
                        if (!activeShippingIds.Contains(shippingId))
                        {
                            var shipping = _dataService.GetById<Shipping>(shippingId);
                            shipping.IsNewCarrierRequest = false;
                        }
                    }

                    result = true;
                }
            }

            return result;
        }

        public void MapFromDtoToEntity(Order entity, OrderDto dto)
        {
            bool isNew = string.IsNullOrEmpty(dto.Id);
            bool isInjection = dto.AdditionalInfo?.Contains("INJECTION") ?? false;

            _mapper.Map(dto, entity);

            if (isNew)
            {
                InitializeNewOrder(entity);
                _historyService.Save(entity.Id, "orderSetDraft", entity.OrderNumber);
            }

            if (isInjection)
            {
                var file = dto.AdditionalInfo.Split(" - ").ElementAtOrDefault(1);
                _historyService.Save(entity.Id, isNew ? "orderCreatedFromInjection" : "orderUpdatedFromInjection", dto.OrderNumber, file);

                if (string.IsNullOrEmpty(entity.Source))
                {
                    entity.Source = file;
                }
                else
                {
                    entity.Source = $"{entity.Source};{file}";
                }
            }
        }

        private void InitializeNewOrder(Order order)
        {
            var userCompanyId = _userProvider.GetCurrentUser()?.CompanyId;
            var shippingWh = order.ShippingWarehouseId == null ? null : _dataService.GetById<ShippingWarehouse>(order.ShippingWarehouseId.Value);

            order.IsActive = true;
            order.Status = OrderState.Draft;
            order.OrderCreationDate = DateTime.UtcNow;
            order.OrderChangeDate = DateTime.UtcNow;
            order.ShippingStatus = VehicleState.VehicleEmpty;
            order.DeliveryStatus = VehicleState.VehicleEmpty;
            order.TemperatureMin = 5;
            order.TemperatureMax = 25;
            order.DocumentReturnStatus = false;
            order.CompanyId = userCompanyId ?? shippingWh?.CompanyId;
        }

        private MapperConfiguration ConfigureMapper()
        {
            var user = _userProvider.GetCurrentUser();
            var lang = user?.Language;

            var result = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OrderDto, OrderFormDto>();

                cfg.CreateMap<OrderDto, Order>()
                    .ForMember(t => t.Id, e => e.Ignore())
                    .ForMember(t => t.Status, e => e.Ignore())
                    .ForMember(t => t.ManualDeliveryCost, e => e.Ignore())
                    .ForMember(t => t.ManualDeliveryDate, e => e.Ignore())
                    .ForMember(t => t.ManualPalletsCount, e => e.Ignore())
                    .ForMember(t => t.ManualPickingTypeId, e => e.Ignore())
                    .ForMember(t => t.ManualShippingDate, e => e.Ignore())
                    .ForMember(t => t.ShippingId, e => e.Ignore())
                    .ForMember(t => t.ShippingNumber, e => e.Ignore())
                    .ForMember(t => t.OrderShippingStatus, e => e.Ignore())
                    .ForMember(t => t.IsNewForConfirmed, e => e.Ignore())
                    .ForMember(t => t.IsNewCarrierRequest, e => e.Ignore())
                    .ForMember(t => t.ShippingWarehouseId, e => e.MapFrom((s) => s.ShippingWarehouseId == null ? null : s.ShippingWarehouseId.Value.ToGuid()))
                    .ForMember(t => t.DeliveryWarehouseId, e => e.MapFrom((s) => s.DeliveryWarehouseId == null ? null : s.DeliveryWarehouseId.Value.ToGuid()))
                    .ForMember(t => t.ClientName, e => e.MapFrom((s) => s.ClientName == null ? null : s.ClientName.Value))
                    .ForMember(t => t.ShippingStatus, e => e.Condition((s) => !string.IsNullOrEmpty(s.ShippingStatus)))
                    .ForMember(t => t.ShippingStatus, e => e.MapFrom((s) => MapFromStateDto<VehicleState>(s.ShippingStatus)))
                    .ForMember(t => t.DeliveryStatus, e => e.Condition((s) => !string.IsNullOrEmpty(s.DeliveryStatus)))
                    .ForMember(t => t.DeliveryStatus, e => e.MapFrom((s) => MapFromStateDto<VehicleState>(s.DeliveryStatus)))
                    .ForMember(t => t.CarrierId, e => e.MapFrom((s) => s.CarrierId == null ? null : s.CarrierId.Value.ToGuid()))
                    .ForMember(t => t.BodyTypeId, e => e.MapFrom((s) => s.BodyTypeId == null ? null : s.BodyTypeId.Value.ToGuid()))
                    .ForMember(t => t.DeliveryType, e => e.MapFrom((s) => s.DeliveryType == null || string.IsNullOrEmpty(s.DeliveryType.Value) ? (DeliveryType?)null : MapFromStateDto<DeliveryType>(s.DeliveryType.Value)))
                    .ForMember(t => t.OrderDate, e => e.MapFrom((s) => s.OrderDate.ToDate()))
                    .ForMember(t => t.OrderType, e => e.MapFrom((s) => s.OrderType == null || string.IsNullOrEmpty(s.OrderType.Value) ? (OrderType?)null : MapFromStateDto<OrderType>(s.OrderType.Value)))
                    .ForMember(t => t.ShippingDate, e => e.MapFrom((s) => s.ShippingDate.ToDateTime()))
                    .ForMember(t => t.DeliveryDate, e => e.MapFrom((s) => s.DeliveryDate.ToDateTime()))
                    .ForMember(t => t.BoxesCount, e => e.MapFrom((s) => Round(s.BoxesCount, 1)))
                    .ForMember(t => t.ConfirmedBoxesCount, e => e.MapFrom((s) => Round(s.ConfirmedBoxesCount, 1)))
                    .ForMember(t => t.PickingTypeId, e => e.MapFrom((s) => s.PickingTypeId == null ? null : s.PickingTypeId.Value.ToGuid()))
                    .ForMember(t => t.LoadingArrivalTime, e => e.MapFrom((s) => s.LoadingArrivalTime.ToDateTime()))
                    .ForMember(t => t.LoadingDepartureTime, e => e.MapFrom((s) => s.LoadingDepartureTime.ToDateTime()))
                    .ForMember(t => t.UnloadingArrivalTime, e => e.MapFrom((s) => s.UnloadingArrivalTime.ToDateTime()))
                    .ForMember(t => t.UnloadingDepartureTime, e => e.MapFrom((s) => s.UnloadingDepartureTime.ToDateTime()))
                    .ForMember(t => t.DocumentsReturnDate, e => e.MapFrom((s) => s.DocumentsReturnDate.ToDate()))
                    .ForMember(t => t.ActualDocumentsReturnDate, e => e.MapFrom((s) => s.ActualDocumentsReturnDate.ToDate()))
                    .ForMember(t => t.PlannedReturnDate, e => e.MapFrom((s) => s.PlannedReturnDate.ToDate()))
                    .ForMember(t => t.ActualReturnDate, e => e.MapFrom((s) => s.ActualReturnDate.ToDate()))
                    .ForMember(t => t.DocumentReturnStatus, e => e.MapFrom((s) => s.DocumentReturnStatus))
                    .ForMember(t => t.IsReturn, e => e.MapFrom((s) => s.IsReturn))
                    .ForMember(t => t.CompanyId, e => e.Condition((s) => s.CompanyId != null))
                    .ForMember(t => t.CompanyId, e => e.MapFrom((s) => s.CompanyId.Value.ToGuid()))
                    .ForMember(t => t.VehicleTypeId, e => e.MapFrom((s) => s.VehicleTypeId == null ? null : s.VehicleTypeId.Value.ToGuid()))
                    .ForMember(t => t.TarifficationType, e => e.MapFrom((s) => s.TarifficationType == null || string.IsNullOrEmpty(s.TarifficationType.Value) ? (TarifficationType?)null : MapFromStateDto<TarifficationType>(s.TarifficationType.Value)))
                    .ForMember(t => t.ShippingWarehouseState, e => e.MapFrom((s) => s.ShippingWarehouseState == null ? default : s.ShippingWarehouseState.Value.ToEnum<WarehouseOrderState>().GetValueOrDefault()));
            });
            return result;
        }

        private T MapFromStateDto<T>(string dtoStatus) where T : struct, Enum
        {
            var mapFromStateDto = dtoStatus.ToEnum<T>() ?? default;
            return mapFromStateDto;
        }

        private decimal? Round(decimal? value, int decimals)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                return decimal.Round(value.Value, decimals);
            }
        }
    }
}
