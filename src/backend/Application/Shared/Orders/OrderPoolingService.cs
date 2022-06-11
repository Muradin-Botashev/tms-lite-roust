using Application.Shared.Addresses;
using Application.Shared.Pooling;
using Application.Shared.Pooling.Models;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.History;
using Domain.Services.Pooling.Models;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Application.Shared.Orders
{
    /// <summary>
    /// Orders pooling integration service
    /// </summary>
    public class OrderPoolingService: IOrderPoolingService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly IPoolingApiService _poolingApiService;
        private readonly ICleanAddressService _cleanAddressService;
        private readonly IHistoryService _historyService;

        public OrderPoolingService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            IPoolingApiService poolingApiService,
            ICleanAddressService cleanAddressService,
            IHistoryService historyService)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _poolingApiService = poolingApiService;
            _cleanAddressService = cleanAddressService;
            _historyService = historyService;
        }

        public HttpResult<SlotDto> GetSlot(Shipping shipping, Order order)
        {
            using (LogContext.PushProperty("Type", "Pooling"))
            {
                var warehouse = _dataService.GetById<Warehouse>(order.DeliveryWarehouseId.Value);
                var shippingWarehouse = _dataService.GetById<ShippingWarehouse>(order.ShippingWarehouseId.Value);
                var carrier = _dataService.GetById<TransportCompany>(order.CarrierId.Value);
                var bodyType = _dataService.GetById<BodyType>(order.BodyTypeId.Value);
                var company = order.CompanyId == null ? null : _dataService.GetById<Company>(order.CompanyId.Value);
                var shippingType = order.TarifficationType == TarifficationType.Milkrun ? TarifficationType.Pooling : order.TarifficationType;

                var dto = new SlotFilterDto
                {
                    DateFrom = order.ShippingDate.Value.ToString("yyyy-MM-dd"),
                    DateTo = order.ShippingDate.Value.ToString("yyyy-MM-dd"),
                    DeliveryDateFrom = order.DeliveryDate.Value.ToString("yyyy-MM-dd"),
                    DeliveryDateTo = order.DeliveryDate.Value.ToString("yyyy-MM-dd"),
                    ShippingRegionId = shippingWarehouse.PoolingRegionId,
                    CarType = bodyType.PoolingId,
                    ProductType = (shipping?.PoolingProductType ?? company?.PoolingProductType ?? default).ToString(),
                    ShippingType = shippingType.ToString(),
                    OnlyAvailable = true
                };

                if (string.IsNullOrEmpty(warehouse.PoolingId))
                {
                    dto.ClientForeignId = warehouse.Id.ToString();
                }
                else
                {
                    dto.ClientId = warehouse.PoolingId;
                }

                if (string.IsNullOrEmpty(carrier.PoolingId))
                {
                    dto.CarrierForeignId = carrier.Id.ToString();
                }
                else
                {
                    dto.CarrierId = carrier.PoolingId;
                }

                if (string.IsNullOrEmpty(warehouse.DistributionCenterId))
                {
                    dto.UnloadingWarehouseForeignId = warehouse.Id.ToString();
                }
                else
                {
                    dto.UnloadingWarehouseId = warehouse.DistributionCenterId;
                }

                var slots = _poolingApiService.GetSlots(dto, company);

                string shippingDate = order.ShippingDate?.ToString("dd.MM.yyyy");
                string deliveryDate = order.DeliveryDate?.ToString("dd.MM.yyyy");

                if (!slots.IsError)
                {
                    string slotIds = string.Join(", ", slots.Result?.Select(x => x.Id) ?? new string[0]);
                    if (string.IsNullOrEmpty(slotIds))
                    {
                        slotIds = "не найдено слотов";
                    }

                    Log.Information("Успешное получение слотов от {Url} из {WarehouseName} ({shippingDate}) в {SoldToNumber} ({deliveryDate}), перевозщик {Title}: {slotIds}",
                                    _poolingApiService.Url, shippingWarehouse.WarehouseName, shippingDate, warehouse.SoldToNumber, deliveryDate, carrier.Title, slotIds);
                }
                else
                {
                    Log.Warning("Ошибка получения слотов от {Url} из {WarehouseName} ({shippingDate}) в {SoldToNumber} ({deliveryDate}), перевозщик {Title}: {Error}",
                                _poolingApiService.Url, shippingWarehouse.WarehouseName, shippingDate, warehouse.SoldToNumber, deliveryDate, carrier.Title, slots.Error);
                }

                return new HttpResult<SlotDto>
                {
                    Error = slots.Error,
                    Result = slots.Result?.FirstOrDefault(),
                    StatusCode = slots.StatusCode
                };
            }
        }

        public HttpResult<SlotDto> GetSlot(string slotId, Guid? companyId)
        {
            using (LogContext.PushProperty("Type", "Pooling"))
            {
                var company = companyId == null ? null : _dataService.GetById<Company>(companyId.Value);
                return _poolingApiService.GetSlot(slotId, company);
            }
        }

        public HttpResult<ReservationRequestDto> BookSlot(Shipping shipping, IEnumerable<Order> allOrders, SlotDto slot)
        {
            using (LogContext.PushProperty("Type", "Pooling"))
            {
                var orders = allOrders.Where(i => i.PalletsCount > 0).ToList();

                var order = orders.FirstOrDefault();
                var bodyType = order.BodyTypeId == null ? null : _dataService.GetById<BodyType>(order.BodyTypeId.Value);
                var company = order.CompanyId == null ? null : _dataService.GetById<Company>(order.CompanyId.Value);
                var carrier = _dataService.GetById<TransportCompany>(order.CarrierId.Value);
                var shippingType = order.TarifficationType == TarifficationType.Milkrun ? TarifficationType.Pooling : order.TarifficationType;

                var loadingPoints = GetLoadingPoints(orders);
                var unloadingPoints = GetUnloadingPoints(orders);

                var pointsValidationResult = ValidatePoints(loadingPoints.Concat(unloadingPoints));
                if (pointsValidationResult != null)
                {
                    return new HttpResult<ReservationRequestDto>
                    {
                        Error = pointsValidationResult.Message,
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }

                var totalPallets = (int)Math.Ceiling(orders.Sum(i => i.PalletsCount.GetValueOrDefault()));

                var warehouse = order.DeliveryWarehouseId == null ? null : _dataService.GetById<Warehouse>(order.DeliveryWarehouseId.Value);

                var dto = new ReservationRequestDto
                {
                    SlotId = slot?.Id,
                    ForeignId = order.ShippingId.ToString(),
                    Units = new PoolingUnitsDto
                    {
                        PositionFrom = 1,
                        PositionTo = totalPallets,
                        Pallets = totalPallets,
                        Boxes = orders.Sum(i => (int)Math.Round(i.BoxesCount.GetValueOrDefault())),
                        Cost = Math.Round(orders.Sum(i => i.OrderAmountExcludingVAT.GetValueOrDefault()), 2),
                        Weight = Math.Round(orders.Sum(i => i.WeightKg.GetValueOrDefault()), 3)
                    },
                    CarType = bodyType?.PoolingId,
                    CarCapacityType = GetCarCapacityType(order),
                    ProductType = (shipping?.PoolingProductType ?? company?.PoolingProductType ?? default).ToString(),
                    ShippingType = shippingType.ToString(),
                    Orders = GetSlotOrders(shipping?.ShippingNumber, orders),
                    LoadingPoints = loadingPoints,
                    UnloadingPoints = unloadingPoints,
                    Temperature = new PoolingTemperatureDto
                    {
                        From = shipping.TemperatureMin,
                        To = shipping.TemperatureMax
                    },
                    ServicesNeeded = new List<string>(),
                    Client = new PoolingIdDto(warehouse?.PoolingId, warehouse?.Id.ToString()),
                    Carrier = new PoolingIdDto(carrier.PoolingId, carrier.Id.ToString())
                };

                if (shipping.TarifficationType == TarifficationType.Ftl && !string.IsNullOrEmpty(warehouse.PoolingId))
                {
                    dto.ServicesNeeded.Add("WithSupplement");
                }

                var bookedSlot = _poolingApiService.BookSlot(dto, company);

                if (bookedSlot.IsError && dto.ServicesNeeded.Any())
                {
                    dto.ServicesNeeded.Clear();
                    bookedSlot = _poolingApiService.BookSlot(dto, company);
                }

                if (!bookedSlot.IsError)
                {
                    Log.Information("Успешное бронирование {PositionTo} паллет слота {SlotId} в {Url}: бронь {Number}",
                                    dto.Units.PositionTo, dto.SlotId, _poolingApiService.Url, bookedSlot.Result?.Number);
                }
                else
                {
                    Log.Warning("Ошибка бронирования {PositionTo} паллет слота {SlotId} в {Url}: {Error}",
                                dto.Units.PositionTo, dto.SlotId, _poolingApiService.Url, bookedSlot.Error);

                    return bookedSlot;
                }

                return bookedSlot;
            }
        }

        public HttpResult<ReservationRequestDto> UpdateSlot(Shipping shipping, IEnumerable<Order> orders = null)
        {
            using (LogContext.PushProperty("Type", "Pooling"))
            {
                if (orders == null)
                {
                    orders = _dataService.GetDbSet<Order>()
                                .Where(i => i.ShippingId == shipping.Id)
                                .Where(i => !string.IsNullOrEmpty(i.BookingNumber))
                                .ToList();
                }

                if (!orders.Any())
                {
                    Log.Warning($"В перевозке {shipping.ShippingNumber} не найдено заказов для обновления брони");
                    return null;
                }

                var order = orders.First();
                var bookingNumber = orders.Select(x => x.BookingNumber).Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault();
                var bodyType = order.BodyTypeId == null ? null : _dataService.GetById<BodyType>(order.BodyTypeId.Value);
                var company = order.CompanyId == null ? null : _dataService.GetById<Company>(order.CompanyId.Value);
                var warehouse = order.DeliveryWarehouseId == null ? null : _dataService.GetById<Warehouse>(order.DeliveryWarehouseId.Value);
                var carrier = _dataService.GetById<TransportCompany>(order.CarrierId.Value);

                var loadingPoints = GetLoadingPoints(orders);
                var unloadingPoints = GetUnloadingPoints(orders);

                var pointsValidationResult = ValidatePoints(loadingPoints.Concat(unloadingPoints));
                if (pointsValidationResult != null)
                {
                    return new HttpResult<ReservationRequestDto>
                    {
                        Error = pointsValidationResult.Message,
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }

                var totalPallets = (int)Math.Ceiling(orders.Sum(i => i.PalletsCount.GetValueOrDefault()));
                var shippingType = order.TarifficationType == TarifficationType.Milkrun ? TarifficationType.Pooling : order.TarifficationType;

                var dto = new ReservationRequestDto
                {
                    Id = shipping.PoolingReservationId,
                    Number = bookingNumber,
                    ForeignId = shipping.Id.ToString(),
                    Units = new PoolingUnitsDto
                    {
                        PositionFrom = 1,
                        PositionTo = totalPallets,
                        Pallets = totalPallets,
                        Boxes = orders.Sum(i => (int)Math.Round(i.BoxesCount.GetValueOrDefault())),
                        Cost = Math.Round(orders.Sum(i => i.OrderAmountExcludingVAT.GetValueOrDefault()), 2),
                        Weight = Math.Round(orders.Sum(i => i.WeightKg.GetValueOrDefault()), 3)
                    },
                    CarType = bodyType?.PoolingId,
                    ProductType = (shipping?.PoolingProductType ?? company?.PoolingProductType ?? default).ToString(),
                    ShippingType = shippingType.ToString(),
                    CarCapacityType = GetCarCapacityType(order),
                    Orders = GetSlotOrders(shipping?.ShippingNumber, orders),
                    LoadingPoints = loadingPoints,
                    UnloadingPoints = unloadingPoints,
                    Temperature = new PoolingTemperatureDto
                    {
                        From = shipping.TemperatureMin,
                        To = shipping.TemperatureMax
                    },
                    ServicesNeeded = new List<string>(),
                    Client = new PoolingIdDto(warehouse?.PoolingId, warehouse?.Id.ToString()),
                    Carrier = new PoolingIdDto(carrier.PoolingId, carrier.Id.ToString())
                };

                if (shipping.TarifficationType == TarifficationType.Ftl && !string.IsNullOrEmpty(warehouse.PoolingId))
                {
                    dto.ServicesNeeded.Add("WithSupplement");
                }

                var bookedSlot = _poolingApiService.UpdateReservation(dto, company);

                if (bookedSlot.IsError && dto.ServicesNeeded.Any())
                {
                    dto.ServicesNeeded.Clear();
                    bookedSlot = _poolingApiService.UpdateReservation(dto, company);
                }

                if (string.IsNullOrEmpty(bookedSlot.Error))
                {
                    Log.Information($"Успешное обновление бронирони {shipping.PoolingReservationId} в {_poolingApiService.Url}");
                }
                else
                {
                    Log.Warning($"Ошибка  обновление брони {shipping.PoolingReservationId} в {_poolingApiService.Url}: {bookedSlot.Error}");
                }

                return bookedSlot;
            }
        }

        private List<ReservationPointDto> GetUnloadingPoints(IEnumerable<Order> orders)
        {
            return orders.GroupBy(x => new { x.DeliveryWarehouseId, x.DeliveryAddress })
                         .Select(x => new
                         {
                             Order = x.First(),
                             DeliveryDate = x.Min(y => y.DeliveryDate),
                             OrderNumbers = x.Select(y => y.ClientOrderNumber).ToList()
                         })
                         .OrderBy(x => x.DeliveryDate)
                         .Select(x => GetBookUnloadingPoint(x.Order, x.DeliveryDate, x.OrderNumbers))
                         .ToList();
        }

        private List<ReservationPointDto> GetLoadingPoints(IEnumerable<Order> orders)
        {
            return orders.GroupBy(x => new { x.ShippingWarehouseId, x.ShippingAddress })
                         .Select(x => new
                         {
                             Order = x.First(),
                             ShippingDate = x.Min(y => y.ShippingDate),
                             OrderNumbers = x.Select(y => y.ClientOrderNumber).ToList()
                         })
                         .OrderBy(x => x.ShippingDate)
                         .Select(x => GetBookLoadingPoint(x.Order, x.ShippingDate, x.OrderNumbers))
                         .ToList();
        }

        private string GetCarCapacityType(Order order)
        {
            var tonnage = _dataService.GetDbSet<VehicleType>()
                        .Include(i => i.Tonnage)
                        .Where(i => i.Id == order.VehicleTypeId)
                        .Select(i => i.Tonnage.WeightKg)
                        .FirstOrDefault();

            if (!tonnage.HasValue) return null;

            return (tonnage.Value / 1000M).ToString("t0.#", CultureInfo.InvariantCulture).Replace(".", "_");
        }

        private PoolingAddressDto GetAddressDto(IAddress addressData, string addressName, string region, string city, string companyName)
        {
            if (addressData == null || addressData.Address?.ToLower() != addressName.ToLower())
            {
                addressData = _cleanAddressService.CleanAddress(addressName);
            }
            var result = new PoolingAddressDto
            {
                FullAddress = addressName,
                PostalCode = addressData?.PostalCode,
                Region = region ?? addressData?.Region,
                City = city ?? addressData?.City,
                Street = addressData?.Street,
                House = addressData?.House,
                CompanyName = companyName
            };
            return result;
        }

        private ValidateResult ValidatePoints(IEnumerable<ReservationPointDto> points)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;
            var errors = new List<string>();
            var fields = new List<string>();
            foreach (var point in points.Where(x => x.Address != null))
            {
                fields.Clear();
                if (string.IsNullOrEmpty(point.Address.PostalCode))
                {
                    fields.Add(nameof(point.Address.PostalCode).ToLowerFirstLetter().Translate(lang));
                }
                if (string.IsNullOrEmpty(point.Address.Region))
                {
                    fields.Add(nameof(point.Address.Region).ToLowerFirstLetter().Translate(lang));
                }
                if (string.IsNullOrEmpty(point.Address.City))
                {
                    fields.Add(nameof(point.Address.City).ToLowerFirstLetter().Translate(lang));
                }
                if (string.IsNullOrEmpty(point.Address.Street))
                {
                    fields.Add(nameof(point.Address.Street).ToLowerFirstLetter().Translate(lang));
                }
                if (string.IsNullOrEmpty(point.Address.House))
                {
                    fields.Add(nameof(point.Address.House).ToLowerFirstLetter().Translate(lang));
                }

                if (fields.Any())
                {
                    errors.Add("IncompleteAddressError".Translate(lang, point.Address.FullAddress, string.Join(", ", fields)));
                }
            }
            if (errors.Any())
            {
                return new ValidateResult(string.Join(' ', errors), Guid.Empty, true);
            }
            else
            {
                return null;
            }
        }

        private ReservationPointDto GetBookLoadingPoint(Order order, DateTime? shippingDate, List<string> orderNumbers)
        {
            var date = shippingDate?.ToString("s", CultureInfo.InvariantCulture);
            var loadingPoint = new ReservationPointDto
            {
                OrderNumbers = orderNumbers,
                Warehouse = new PoolingIdDto(),
                DateTime = new PoolingDateRangeDto
                {
                    From = date,
                    To = date
                }
            };

            var shippingWarehouse = order.ShippingWarehouseId == null ? null : _dataService.GetById<ShippingWarehouse>(order.ShippingWarehouseId.Value);
            if (shippingWarehouse != null)
            {
                loadingPoint.Warehouse = new PoolingIdDto(
                    id: string.IsNullOrEmpty(shippingWarehouse.PoolingConsolidationId)
                            ? shippingWarehouse.PoolingId
                            : shippingWarehouse.PoolingConsolidationId,
                    foreignId: order.ShippingWarehouseId.ToString()
                );
            }

            if (string.IsNullOrEmpty(shippingWarehouse?.PoolingConsolidationId)
                && string.IsNullOrEmpty(shippingWarehouse?.PoolingId))
            {
                loadingPoint.Address = GetAddressDto(shippingWarehouse, order.ShippingAddress, order.ShippingRegion, order.ShippingCity, null);
            }

            return loadingPoint;
        }

        private ReservationPointDto GetBookUnloadingPoint(Order order, DateTime? deliveryDate, List<string> orderNumbers)
        {
            var date = deliveryDate?.ToString("s", CultureInfo.InvariantCulture);
            var unloadingPoint = new ReservationPointDto
            {
                OrderNumbers = orderNumbers,
                Warehouse = new PoolingIdDto(),
                DateTime = new PoolingDateRangeDto
                {
                    From = date,
                    To = date
                }
            };

            var deliveryWarehouse = order.DeliveryWarehouseId == null ? null : _dataService.GetById<Warehouse>(order.DeliveryWarehouseId.Value);
            if (deliveryWarehouse != null)
            {
                unloadingPoint.Warehouse = new PoolingIdDto(
                    id: deliveryWarehouse.DistributionCenterId,
                    foreignId: order.DeliveryWarehouseId.ToString()
                );
            }

            if (string.IsNullOrEmpty(deliveryWarehouse?.DistributionCenterId))
            {
                unloadingPoint.Address = GetAddressDto(deliveryWarehouse, order.DeliveryAddress, order.DeliveryRegion, order.DeliveryCity, order.ClientName);
            }

            return unloadingPoint;
        }

        private List<ReservationOrderDto> GetSlotOrders(string shippingNumber, IEnumerable<Order> orders)
        {
            var result = new List<ReservationOrderDto>();
            decimal currentPalletsSum = 0M;

            foreach (var order in orders)
            {
                int palletFrom = (int)Math.Floor(currentPalletsSum) + 1;
                currentPalletsSum += order.PalletsCount ?? 0M;
                int palletTo = (int)Math.Ceiling(currentPalletsSum);

                var entry = new ReservationOrderDto
                {
                    Number = order.ClientOrderNumber,
                    WaybillNumber = order.OrderNumber,
                    ConsignorNumber = shippingNumber,
                    PackingList = order.OrderNumber,
                    Units = new PoolingUnitsDto
                    {
                        PositionFrom = palletFrom,
                        PositionTo = palletTo,
                        Boxes = (int)Math.Round(order.BoxesCount.GetValueOrDefault()),
                        Cost = Math.Round(order.OrderAmountExcludingVAT.GetValueOrDefault(), 2),
                        Weight = Math.Round(order.WeightKg.GetValueOrDefault(), 3)
                    }
                };
                result.Add(entry);
            }

            return result;
        }

        /// <summary>
        /// Check required fields
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        private IEnumerable<string> CheckRequiredFieldsInner(IEnumerable<Order> orders)
        {
            var order = orders.First();
            var isPooling = order.TarifficationType == TarifficationType.Pooling || order.TarifficationType == TarifficationType.Milkrun;

            if (orders.Any(i => !i.DeliveryDate.HasValue))
            {
                yield return nameof(Order.DeliveryDate);
            }

            if (orders.Any(i => !i.ShippingDate.HasValue))
            {
                yield return nameof(Order.ShippingDate);
            }

            if (isPooling && orders.Any(i => !i.ShippingWarehouseId.HasValue))
            {
                yield return nameof(Order.ShippingWarehouseId);
            }

            if (!isPooling && orders.Any(i => string.IsNullOrEmpty(i.ShippingAddress)))
            {
                yield return nameof(Order.ShippingAddress);
            }

            if (isPooling && orders.Any(i => !i.DeliveryWarehouseId.HasValue))
            {
                yield return nameof(Order.DeliveryWarehouseId);
            }

            if (!isPooling && orders.Any(i => string.IsNullOrEmpty(i.DeliveryAddress)))
            {
                yield return nameof(Order.DeliveryAddress);
            }

            if (orders.Any(i => string.IsNullOrEmpty(i.DeliveryAddress)))
            {
                yield return nameof(Order.DeliveryAddress);
            }

            if (orders.Any(i => !i.CarrierId.HasValue))
            {
                yield return nameof(Order.CarrierId);
            }

            if (orders.Any(i => !i.BodyTypeId.HasValue))
            {
                yield return nameof(Order.BodyTypeId);
            }

            if (orders.Any(i => !i.OrderAmountExcludingVAT.HasValue || i.OrderAmountExcludingVAT <= 0))
            {
                yield return nameof(Order.OrderAmountExcludingVAT);
            }

            if (orders.Any(i => string.IsNullOrEmpty(i.OrderNumber)))
            {
                yield return nameof(Order.OrderNumber);
            }

            if (orders.Any(i => string.IsNullOrEmpty(i.ClientOrderNumber)))
            {
                yield return nameof(Order.ClientOrderNumber);
            }

            if (!isPooling && string.IsNullOrEmpty(order.ClientName))
            {
                var warehouse = _dataService.GetById<Warehouse>(order.DeliveryWarehouseId.Value);
                if (string.IsNullOrEmpty(warehouse.DistributionCenterId))
                {
                    yield return nameof(Order.ClientName);
                }
            }
        }

        /// <summary>
        /// Check required fields
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        private IEnumerable<string> CheckFieldsMatchInner(IEnumerable<Order> orders)
        {
            var order = orders.First();
            var isPooling = order.TarifficationType == TarifficationType.Pooling || order.TarifficationType == TarifficationType.Milkrun;

            if (isPooling && orders.Any(i => order.DeliveryDate?.Date != i.DeliveryDate?.Date))
            {
                yield return nameof(Order.DeliveryDate).ToLowerFirstLetter();
            }

            if (isPooling && orders.Any(i => order.ShippingDate?.Date != i.ShippingDate?.Date))
            {
                yield return nameof(Order.ShippingDate);
            }

            if (isPooling && orders.Any(i => order.ShippingWarehouseId != i.ShippingWarehouseId))
            {
                yield return nameof(Order.ShippingWarehouseId).ToLowerFirstLetter();
            }

            if (isPooling && orders.Any(i => order.DeliveryWarehouseId != i.DeliveryWarehouseId))
            {
                yield return nameof(Order.DeliveryWarehouseId).ToLowerFirstLetter();
            }

            if (isPooling && orders.Any(i => order.DeliveryAddress != i.DeliveryAddress))
            {
                yield return nameof(Order.ClientName).ToLowerFirstLetter();
            }

            if (orders.All(i => order.CarrierId != i.CarrierId))
            {
                yield return nameof(Order.CarrierId).ToLowerFirstLetter();
            }

            if (orders.All(i => order.BodyTypeId != i.BodyTypeId))
            {
                yield return nameof(Order.BodyTypeId).ToLowerFirstLetter();
            }
        }

        private IEnumerable<string> CheckRelatedValues(Order order)
        {
            var isPooling = order.TarifficationType == TarifficationType.Pooling || order.TarifficationType == TarifficationType.Milkrun;
            if (isPooling)
            {
                var warehouse = _dataService.GetById<Warehouse>(order.DeliveryWarehouseId.Value);

                if (string.IsNullOrEmpty(warehouse.PoolingId))
                {
                    yield return nameof(Warehouse).ToLowerFirstLetter();
                }

                if (string.IsNullOrEmpty(warehouse.DistributionCenterId))
                {
                    yield return nameof(Warehouse).ToLowerFirstLetter();
                }

                var shippingWarehouse = _dataService.GetById<ShippingWarehouse>(order.ShippingWarehouseId.Value);

                if (string.IsNullOrEmpty(shippingWarehouse.PoolingRegionId))
                {
                    yield return nameof(ShippingWarehouse).ToLowerFirstLetter();
                }

                if (string.IsNullOrEmpty(shippingWarehouse.PoolingId))
                {
                    yield return nameof(ShippingWarehouse).ToLowerFirstLetter();
                }
            }

            var carrier = _dataService.GetById<TransportCompany>(order.CarrierId.Value);

            if (string.IsNullOrEmpty(carrier.PoolingId))
            { 
                yield return nameof(TransportCompany).ToLowerFirstLetter();
            }

            var bodyType = _dataService.GetById<BodyType>(order.BodyTypeId.Value);

            if (string.IsNullOrEmpty(bodyType.PoolingId))
            {
                yield return nameof(BodyType).ToLowerFirstLetter();
            }
        }

        /// <summary>
        /// Check related values
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        private IEnumerable<string> CheckRelatedValues(IEnumerable<Order> orders)
        {
            return orders.SelectMany(CheckRelatedValues).Distinct();
        }

        /// <summary>
        /// Cancel slot reservation
        /// </summary>
        /// <param name="shipping"></param>
        /// <returns></returns>
        public HttpResult CancelSlot(Shipping shipping)
        {
            using (LogContext.PushProperty("Type", "Pooling"))
            {
                var number = _dataService.GetDbSet<Order>()
                    .Where(i => i.ShippingId == shipping.Id)
                    .Select(i => i.BookingNumber)
                    .FirstOrDefault();

                var company = shipping.CompanyId == null ? null : _dataService.GetById<Company>(shipping.CompanyId.Value);

                var result = _poolingApiService.CancelSlot(shipping.PoolingReservationId, number, shipping.Id.ToString(), company);

                if (!result.IsError)
                {
                    Log.Information($"Успешная отмена слота { shipping.SlotId } в { _poolingApiService.Url }, бронь { number }");
                }
                else
                {
                    Log.Warning($"Ошибка отмены слота { shipping.SlotId } в { _poolingApiService.Url }, бронь { number }: { result.Error }");
                    return result;
                }


                shipping.Status = ShippingState.ShippingSlotCancelled;
                shipping.PoolingReservationId = null;
                shipping.SlotId = null;
                shipping.ConsolidationDate = null;
                shipping.IsPooling = null;

                var orders = _dataService.GetDbSet<Order>()
                    .Where(i => i.ShippingId == shipping.Id).ToList();

                foreach (var order in orders)
                {
                    order.OrderShippingStatus = ShippingState.ShippingSlotCancelled;
                    order.BookingNumber = null;
                    order.IsPooling = null;
                }

                return result;
            }
        }

        public AppResult ValidateGetSlot(HttpResult<SlotDto> slot, CurrentUserDto user)
        {
            using (LogContext.PushProperty("Type", "Pooling"))
            {
                if (slot == null || (!slot.IsError && slot.Result == null))
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = "poolingBookedSlotNotFound".Translate(user.Language),
                        ManuallyClosableMessage = true
                    };
                }

                var errorsMap = new Dictionary<HttpStatusCode, string>
                {
                    { HttpStatusCode.Unauthorized, "poolingSlotUnauthorized" },
                    { HttpStatusCode.InternalServerError, "poolingSlotInternalServerError" },
                };

                if (errorsMap.ContainsKey(slot.StatusCode))
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = errorsMap[slot.StatusCode].Translate(user.Language)
                    };
                }
                else if (!string.IsNullOrEmpty(slot.Error))
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = slot.Error,
                        ManuallyClosableMessage = true
                    };
                }
                else if (slot.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = "poolingSlotBadRequest".Translate(user.Language),
                        ManuallyClosableMessage = true
                    };
                }

                return null;
            }
        }

        public AppResult ValidateBookedSlot(HttpResult<ReservationRequestDto> slot, CurrentUserDto user)
        {
            using (LogContext.PushProperty("Type", "Pooling"))
            {
                var errorsMap = new Dictionary<HttpStatusCode, string>
                {
                    { HttpStatusCode.Unauthorized, "poolingSlotUnauthorized" },
                    { HttpStatusCode.Forbidden, "poolingBookedSlotForbidden" },
                    { HttpStatusCode.NotFound, "poolingBookedSlotNotFound" },
                    { HttpStatusCode.InternalServerError, "poolingSlotInternalServerError" },
                };

                if (errorsMap.ContainsKey(slot.StatusCode))
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = errorsMap[slot.StatusCode].Translate(user.Language),
                        ManuallyClosableMessage = true
                    };
                }
                else if (!string.IsNullOrEmpty(slot.Error))
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = slot.Error,
                        ManuallyClosableMessage = true
                    };
                }
                else if (slot.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = "poolingSlotBadRequest".Translate(user.Language),
                        ManuallyClosableMessage = true
                    };
                }

                return null;
            }
        }

        public AppResult ValidateCancelSlot(HttpResult result, Shipping shipping, CurrentUserDto user)
        {
            using (LogContext.PushProperty("Type", "Pooling"))
            {
                var errorsMap = new Dictionary<HttpStatusCode, string>
                {
                    { HttpStatusCode.Unauthorized, "poolingSlotUnauthorized" },
                    { HttpStatusCode.Forbidden, "poolingCancelSlotForbidden" },
                    { HttpStatusCode.NotFound, "poolingCancelSlotNotFound" },
                    { HttpStatusCode.InternalServerError, "poolingCancelSlotInternalServerError" },
                };

                var errorрHistoryMap = new Dictionary<HttpStatusCode, string>
                {
                    { HttpStatusCode.Unauthorized, "poolingSlotHistoryUnauthorized" },
                    { HttpStatusCode.Forbidden, "poolingCancelSlotHistoryForbidden" },
                    { HttpStatusCode.NotFound, "poolingCancelSlotHistoryNotFound" },
                    { HttpStatusCode.InternalServerError, "poolingCancelSlotHistoryInternalServerError" },
                };

                if (errorsMap.ContainsKey(result.StatusCode))
                {
                    _historyService.Save(shipping.Id, errorрHistoryMap[result.StatusCode]);

                    return new AppResult
                    {
                        IsError = true,
                        Message = errorsMap[result.StatusCode].Translate(user.Language)
                    };
                }

                if (!result.IsError) return null;

                return new AppResult
                {
                    IsError = true,
                    Message = result.Error
                };
            }
        }

        public bool CheckConsolidationDate(Shipping shipping)
        { 
            return !shipping.ConsolidationDate.HasValue || shipping.ConsolidationDate.Value.AddDays(-1).Date.AddHours(17) >= DateTime.Now;
        }

        public AppResult ValidateOrders(IEnumerable<Order> orders, CurrentUserDto user)
        {
            using (LogContext.PushProperty("Type", "Pooling"))
            {
                var requiredFieldsResult = RequiredFieldsCheck(orders, user);

                if (requiredFieldsResult != null)
                {
                    return requiredFieldsResult;
                }

                var relatedValuesResult = RelatedValuesCheck(orders, user);

                if (relatedValuesResult != null)
                {
                    return relatedValuesResult;
                }

                var matchedValuesResult = FieldsMatchCheck(orders, user);

                if (matchedValuesResult != null)
                {
                    return matchedValuesResult;
                }

                if (orders.Any(i => i.PalletsCount <= 0))
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = "poolingInvalidPalletsCount".Translate(user.Language),
                        ManuallyClosableMessage = true
                    };
                }

                if (orders.Any(i => i.TarifficationType == TarifficationType.Doubledeck))
                {
                    return new AppResult
                    {
                        IsError = true,
                        Message = "poolingInvalidTarifficationType".Translate(user.Language),
                        ManuallyClosableMessage = true
                    };
                }

                return null;
            }
        }

        private AppResult RequiredFieldsCheck(IEnumerable<Order> orders, CurrentUserDto user)
        {
            var requiredFields = CheckRequiredFieldsInner(orders);

            var errors = new List<string>();

            if (requiredFields.Any())
            {
                var requiredFieldsTranslations = requiredFields.Select(i => i.ToLowerFirstLetter().Translate(user.Language));
                errors.Add("orderSendToPoolingRequiredFields".Translate(user.Language, string.Join(", ", requiredFieldsTranslations)));
            }

            if (errors.Any())
            {
                return new AppResult
                {
                    IsError = true,
                    Message = string.Join("; ", errors),
                    ManuallyClosableMessage = true
                };
            }

            return null;
        }

        private AppResult FieldsMatchCheck(IEnumerable<Order> orders, CurrentUserDto user)
        {
            var requiredFields = CheckFieldsMatchInner(orders);

            var errors = new List<string>();

            if (requiredFields.Any())
            {
                var requiredFieldsTranslations = requiredFields.Select(i => i.ToLowerFirstLetter().Translate(user.Language));
                errors.Add("orderSendToPoolingFieldsMatch".Translate(user.Language, string.Join(", ", requiredFieldsTranslations)));
            }

            if (errors.Any())
            {
                return new AppResult
                {
                    IsError = true,
                    Message = string.Join("; ", errors),
                    ManuallyClosableMessage = true
                };
            }

            return null;
        }

        private AppResult RelatedValuesCheck(IEnumerable<Order> orders, CurrentUserDto user)
        {
            var relatedValuesRequired = CheckRelatedValues(orders).Distinct();

            if (relatedValuesRequired.Count() == 1)
            {
                var fieldTranslation = relatedValuesRequired.First().Translate(user.Language);
                return new AppResult
                {
                    IsError = true,
                    Message = "orderSendToPoolingRelatedRequiredSingle".Translate(user.Language, fieldTranslation),
                    ManuallyClosableMessage = true
                };
            }
            else if (relatedValuesRequired.Count() > 1)
            {
                var fieldTranslations = relatedValuesRequired.Select(i => i.Translate(user.Language));
                return new AppResult
                {
                    IsError = true,
                    Message = "orderSendToPoolingRelatedRequiredMultiple".Translate(user.Language, string.Join(", ", fieldTranslations)),
                    ManuallyClosableMessage = true
                };
            }

            return null;
        }
    }
}
