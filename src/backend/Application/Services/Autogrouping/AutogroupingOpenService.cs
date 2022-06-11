using Application.Shared.Addresses;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Autogrouping;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Autogrouping
{
    public class AutogroupingOpenService : IAutogroupingOpenService
    {
        private ICommonDataService _dataService;
        private IGroupingOrdersService _groupingOrdersService;
        private ICleanAddressService _cleanAddressService;
        private IUserProvider _userProvider;

        public AutogroupingOpenService(
            ICommonDataService dataService, 
            IGroupingOrdersService groupingOrdersService,
            ICleanAddressService cleanAddressService,
            IUserProvider userProvider)
        {
            _dataService = dataService;
            _groupingOrdersService = groupingOrdersService;
            _cleanAddressService = cleanAddressService;
            _userProvider = userProvider;
        }

        public OpenRunResponse RunGrouping(OpenRunRequest request)
        {
            var result = ValidateRequiredFields(request);
            if (result.IsError)
            {
                return result;
            }

            var orders = ConvertWaybills(request, ref result);
            List<AutogroupingType> types = ConvertAutogroupingTypes(request, ref result);
            if (result.IsError)
            {
                return result;
            }

            Dictionary<IAutogroupingOrder, string> skippedOrders;
            var resultData = _groupingOrdersService.GroupOrders(orders, Guid.NewGuid(), types, out skippedOrders);

            SaveGroupedWaybills(resultData, ref result);
            SaveSkippedWaybills(skippedOrders, ref result);

            return result;
        }

        private OpenRunResponse ValidateRequiredFields(OpenRunRequest request)
        {
            var response = new OpenRunResponse();
            var lang = _userProvider.GetCurrentUser()?.Language;

            if (request?.Waybills == null)
            {
                var errorMessage = "Autogrouping.Open.EmptyWaybills".Translate(lang);
                response.AddError(nameof(request.Waybills).ToLowerFirstLetter(), errorMessage, ValidationErrorType.ValueIsRequired);
            }
            else
            {
                for (int ind = 0; ind < request.Waybills.Count; ind++)
                {
                    var waybill = request.Waybills[ind];

                    if (string.IsNullOrEmpty(waybill?.BodyType))
                    {
                        AddEmptyWaybillFieldError(response, lang, ind + 1, nameof(waybill.BodyType).ToLowerFirstLetter());
                    }

                    if (string.IsNullOrEmpty(waybill?.DeliveryAddress))
                    {
                        AddEmptyWaybillFieldError(response, lang, ind + 1, nameof(waybill.DeliveryAddress).ToLowerFirstLetter());
                    }

                    if (waybill?.DeliveryDate == null)
                    {
                        AddEmptyWaybillFieldError(response, lang, ind + 1, nameof(waybill.DeliveryDate).ToLowerFirstLetter());
                    }

                    if (string.IsNullOrEmpty(waybill?.Number))
                    {
                        AddEmptyWaybillFieldError(response, lang, ind + 1, nameof(waybill.Number).ToLowerFirstLetter());
                    }

                    if (waybill?.PalletsCount == null)
                    {
                        AddEmptyWaybillFieldError(response, lang, ind + 1, nameof(waybill.PalletsCount).ToLowerFirstLetter());
                    }

                    if (string.IsNullOrEmpty(waybill?.ShippingAddress))
                    {
                        AddEmptyWaybillFieldError(response, lang, ind + 1, nameof(waybill.ShippingAddress).ToLowerFirstLetter());
                    }

                    if (waybill?.ShippingDate == null)
                    {
                        AddEmptyWaybillFieldError(response, lang, ind + 1, nameof(waybill.ShippingDate).ToLowerFirstLetter());
                    }

                    if (waybill?.Weight == null)
                    {
                        AddEmptyWaybillFieldError(response, lang, ind + 1, nameof(waybill.Weight).ToLowerFirstLetter());
                    }
                }
            }

            return response;
        }

        private List<OrderDto> ConvertWaybills(OpenRunRequest request, ref OpenRunResponse response)
        {
            var currentUser = _userProvider.GetCurrentUser();
            var lang = currentUser?.Language;

            var shippingWarehouseIds = request.Waybills.Select(x => x.ShippingWarehouseId.ToGuid())
                                                       .Where(x => x != null)
                                                       .ToList();
            var shippingWarehousesById = _dataService.GetDbSet<ShippingWarehouse>()
                                                     .Where(x => shippingWarehouseIds.Contains(x.Id))
                                                     .ToDictionary(x => x.Id);

            var shippingAddresses = request.Waybills.Select(x => x.ShippingAddress)
                                                    .Where(x => !string.IsNullOrEmpty(x))
                                                    .ToList();
            var shippingWarehousesList = _dataService.GetDbSet<ShippingWarehouse>()
                                                     .Where(x => shippingAddresses.Contains(x.Address))
                                                     .ToList();
            var shippingWarehousesByAddress = new Dictionary<string, ShippingWarehouse>();
            foreach (var entry in shippingWarehousesList)
            {
                shippingWarehousesByAddress[entry.Address] = entry;
            }

            var deliveryWarehouseIds = request.Waybills.Select(x => x.DeliveryWarehouseId.ToGuid())
                                                       .Where(x => x != null)
                                                       .ToList();
            var deliveryWarehousesById = _dataService.GetDbSet<Warehouse>()
                                                     .Where(x => deliveryWarehouseIds.Contains(x.Id))
                                                     .ToDictionary(x => x.Id);

            var deliveryAddresses = request.Waybills.Select(x => x.DeliveryAddress)
                                                    .Where(x => !string.IsNullOrEmpty(x))
                                                    .ToList();
            var deliveryWarehousesList = _dataService.GetDbSet<Warehouse>()
                                                     .Where(x => deliveryAddresses.Contains(x.Address))
                                                     .ToList();
            var deliveryWarehousesByAddress = new Dictionary<string, Warehouse>();
            foreach (var entry in deliveryWarehousesList)
            {
                deliveryWarehousesByAddress[entry.Address] = entry;
            }

            var bodyTypesDict = _dataService.GetDbSet<BodyType>()
                                            .Where(x => x.IsActive && x.PoolingId != null && x.PoolingId.Length > 0)
                                            .ToDictionary(x => x.PoolingId.ToLower());

            var addresses = request.Waybills.SelectMany(x => new[] { x.ShippingAddress, x.DeliveryAddress }).Distinct().ToList();
            var cleanAddressesDict = new Dictionary<string, CleanAddressDto>();
            foreach (var address in addresses)
            {
                cleanAddressesDict[address] = _cleanAddressService.CleanAddress(address);
            }

            var processedWaybillNumbers = new HashSet<string>();

            var orders = new List<OrderDto>();
            for (int ind = 0; ind < request.Waybills.Count; ind++)
            {
                var waybill = request.Waybills[ind];

                if (processedWaybillNumbers.Contains(waybill.Number))
                {
                    var errorMessage = "Autogrouping.Open.DuplicateWaybillNumber".Translate(lang, ind + 1, waybill.Number);
                    response.AddError(nameof(waybill.Number).ToLowerFirstLetter(), errorMessage, ValidationErrorType.DuplicatedRecord);
                }
                processedWaybillNumbers.Add(waybill.Number);

                ShippingWarehouse shippingWarehouse = null;
                var shippingWarehouseId = waybill.ShippingWarehouseId.ToGuid();
                if (shippingWarehouseId != null)
                {
                    shippingWarehousesById.TryGetValue(shippingWarehouseId.Value, out shippingWarehouse);
                }
                if (shippingWarehouse == null)
                {
                    shippingWarehousesByAddress.TryGetValue(waybill.ShippingAddress, out shippingWarehouse);
                }

                Warehouse deliveryWarehouse = null;
                var deliveryWarehouseId = waybill.DeliveryWarehouseId.ToGuid();
                if (deliveryWarehouseId != null)
                {
                    deliveryWarehousesById.TryGetValue(deliveryWarehouseId.Value, out deliveryWarehouse);
                }
                if (deliveryWarehouse == null)
                {
                    deliveryWarehousesByAddress.TryGetValue(waybill.DeliveryAddress, out deliveryWarehouse);
                }

                BodyType bodyType = null;
                if (!bodyTypesDict.TryGetValue(waybill.BodyType.ToLower(), out bodyType))
                {
                    AddInvalidWaybillFieldError(response, lang, ind + 1, nameof(waybill.BodyType).ToLowerFirstLetter());
                }

                CleanAddressDto shippingAddress = null;
                cleanAddressesDict.TryGetValue(waybill.ShippingAddress, out shippingAddress);

                CleanAddressDto deliveryAddress = null;
                cleanAddressesDict.TryGetValue(waybill.DeliveryAddress, out deliveryAddress);

                var order = new OrderDto
                {
                    BodyTypeId = bodyType?.Id,
                    CompanyId = currentUser?.CompanyId,
                    DeliveryAddress = waybill.DeliveryAddress,
                    DeliveryCity = deliveryAddress?.City,
                    DeliveryDate = waybill.DeliveryDate,
                    DeliveryRegion = deliveryAddress?.Region,
                    DeliveryWarehouse = deliveryWarehouse,
                    DeliveryWarehouseId = deliveryWarehouseId,
                    Id = Guid.NewGuid(),
                    OrderNumber = waybill.Number,
                    PalletsCount = waybill.PalletsCount,
                    ShippingAddress = waybill.ShippingAddress,
                    ShippingCity = shippingAddress?.City,
                    ShippingDate = waybill.ShippingDate,
                    ShippingRegion = shippingAddress?.Region,
                    ShippingWarehouse = shippingWarehouse,
                    ShippingWarehouseId = shippingWarehouseId,
                    Status = OrderState.Created,
                    WeightKg = waybill.Weight
                };
                orders.Add(order);
            }

            return orders;
        }

        private List<AutogroupingType> ConvertAutogroupingTypes(OpenRunRequest request, ref OpenRunResponse response)
        {
            if (request?.AutogroupingTypes == null)
            {
                return null;
            }

            var result = new List<AutogroupingType>();
            var invalidNames = new List<string>();
            foreach (var typeName in request.AutogroupingTypes)
            {
                var type = typeName.ToEnum<AutogroupingType>();
                if (type == null)
                {
                    invalidNames.Add(typeName);
                }
                else
                {
                    result.Add(type.Value);
                }
            }

            if (invalidNames.Any())
            {
                var lang = _userProvider.GetCurrentUser()?.Language;
                var errorMessage = "Autogrouping.Open.InvalidAutogroupingType".Translate(lang, string.Join(", ", invalidNames));
                response.AddError(nameof(request.AutogroupingTypes).ToLowerFirstLetter(), errorMessage, ValidationErrorType.InvalidDictionaryValue);
            }

            return result;
        }

        private void AddEmptyWaybillFieldError(OpenRunResponse response, string lang, int ind, string field)
        {
            var errorMessage = "Autogrouping.Open.EmptyWaybillField".Translate(lang, ind, field);
            response.AddError(field, errorMessage, ValidationErrorType.ValueIsRequired);
        }

        private void AddInvalidWaybillFieldError(OpenRunResponse response, string lang, int ind, string field)
        {
            var errorMessage = "Autogrouping.Open.InvalidWaybillField".Translate(lang, ind, field);
            response.AddError(field, errorMessage, ValidationErrorType.ValueIsRequired);
        }

        private void SaveGroupedWaybills(AutogroupingResultData resultData, ref OpenRunResponse result)
        {
            if (resultData?.Shippings?.Count > 0)
            {
                result.Shippings = new List<OpenRunShipping>();

                var carriersDict = _dataService.GetDbSet<TransportCompany>().ToDictionary(x => x.Id);
                var waybillNumbersDict = resultData.Orders.GroupBy(x => x.AutogroupingShippingId)
                                                          .ToDictionary(x => x.Key, x => x.Select(y => y.OrderNumber).ToList());

                foreach (var shippingData in resultData.Shippings)
                {
                    TransportCompany carrier = null;
                    if (shippingData.CarrierId != null)
                    {
                        carriersDict.TryGetValue(shippingData.CarrierId.Value, out carrier);
                    }

                    waybillNumbersDict.TryGetValue(shippingData.Id, out List<string> waybillNumbers);

                    var shipping = new OpenRunShipping
                    {
                        ShippingNumber = shippingData.ShippingNumber,
                        AutogroupingType = shippingData.AutogroupingType.FormatEnum(),
                        Carrier = carrier?.Title,
                        DeliveryCost = shippingData.BestCost,
                        WaybillNumbers = waybillNumbers
                    };
                    result.Shippings.Add(shipping);
                }
            }
        }

        private void SaveSkippedWaybills(Dictionary<IAutogroupingOrder, string> skippedOrders, ref OpenRunResponse result)
        {
            if (skippedOrders?.Count > 0)
            {
                result.SkippedWaybills = new List<OpenRunSkippedWaybill>();
                foreach (var skippedOrder in skippedOrders)
                {
                    var skippedWaybill = new OpenRunSkippedWaybill
                    {
                        Number = skippedOrder.Key.OrderNumber,
                        Errors = skippedOrder.Value
                    };
                    result.SkippedWaybills.Add(skippedWaybill);
                }
            }
        }
    }
}
