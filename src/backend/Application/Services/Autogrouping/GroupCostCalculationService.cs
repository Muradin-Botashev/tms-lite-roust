using Application.Shared.Distances;
using Application.Shared.Pooling;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.Pooling.Models;
using Domain.Services.Translations;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Autogrouping
{
    public class GroupCostCalculationService : IGroupCostCalculationService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly IWarehouseDistancesService _distancesService;
        private readonly IPoolingApiService _poolingApiService;

        public GroupCostCalculationService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            IWarehouseDistancesService distancesService,
            IPoolingApiService poolingApiService)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _distancesService = distancesService;
            _poolingApiService = poolingApiService;
        }

        /// <summary>
        /// Заполнение стоимостей по маршрутам и входящим в них псевдоперевозкам
        /// </summary>
        public void FillCosts(IEnumerable<ShippingRoute> routes, List<AutogroupingType> types)
        {
            var tariffs = _dataService.GetDbSet<Tariff>().Include(x => x.Carrier).ToList();

            foreach (var route in routes)
            {
                var companyId = route.Shippings.First().CompanyId;
                var companyTariffs = tariffs.Where(x => x.CompanyId == companyId).ToList();

                foreach (var shipping in route.Shippings)
                {
                    FillPseudoShippingCosts(shipping, companyTariffs, types);
                }

                FillShippingRouteCost(route, companyTariffs, types);
            }
        }

        /// <summary>
        /// Заполнение стоимости по псевдоперевозке
        /// </summary>
        private void FillPseudoShippingCosts(PseudoShipping shipping, List<Tariff> tariffs, List<AutogroupingType> types)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;
            var settings = _dataService.GetDbSet<AutogroupingSetting>()
                                       .FirstOrDefault(x => x.CompanyId == shipping.CompanyId);

            var canCalcPooling = true;
            if (settings?.CheckPoolingSlots == true)
            {
                canCalcPooling = HasPoolingSlots(shipping);
            }

            var palletsCount = (int)Math.Ceiling(shipping.PalletsCount);
            shipping.Costs = shipping.Costs ?? new Dictionary<TarifficationType, CostData>();
            foreach (var tarifficationType in Enum.GetValues(typeof(TarifficationType)).Cast<TarifficationType>())
            {
                if (tarifficationType == TarifficationType.Doubledeck)
                {
                    continue;
                }

                var autogroupingType = GetDirectAutogroupingType(tarifficationType);

                var costValue = new CostData();
                var allCostValues = new List<CostData>();

                if (types?.Count > 0 && !types.Contains(autogroupingType))
                {
                    costValue.Message = "autogrouping.TypeDisabled".Translate(lang);
                }
                else if ((tarifficationType == TarifficationType.Pooling || tarifficationType == TarifficationType.Milkrun) && !canCalcPooling)
                {
                    costValue.Value = 0M;
                    costValue.Message = "autogrouping.PoolingNoSlots".Translate(lang);
                }
                else if (tarifficationType == TarifficationType.Milkrun && shipping.PalletsCount < 15)
                {
                    costValue.Message = "autogrouping.MilkrunNotEnoughPallets".Translate(lang);
                }
                else
                {
                    var bestTariffs = FindBestTariffs(shipping, tarifficationType, shipping.VehicleType, tariffs, 1);

                    var bestTariff = bestTariffs.FirstOrDefault();
                    var cost = CalculateTariff(bestTariff, tarifficationType, shipping.ShippingDate, palletsCount);
                    if (cost != null)
                    {
                        costValue.Value = cost.Value;
                        costValue.CarrierId = bestTariff.CarrierId;
                        costValue.IsValid = true;
                    }
                    else
                    {
                        costValue.Value = 0M;
                        costValue.Message = "autogrouping.TariffNotFound".Translate(lang);
                    }

                    foreach (var tariff in bestTariffs)
                    {
                        var tariffCost = CalculateTariff(tariff, tarifficationType, shipping.ShippingDate, palletsCount);
                        if (tariffCost != null)
                        {
                            allCostValues.Add(new CostData
                            {
                                CarrierId = tariff.CarrierId,
                                Value = tariffCost
                            });
                        }
                    }
                }

                shipping.Costs[tarifficationType] = costValue;
                shipping.AllCosts[autogroupingType] = allCostValues;
            }
        }

        /// <summary>
        /// Проверка наличия слотов в Pooling под псевдоперевозку
        /// </summary>
        private bool HasPoolingSlots(PseudoShipping shipping)
        {
            using (LogContext.PushProperty("Type", "Pooling"))
            {
                var order = shipping.Orders.FirstOrDefault();

                if (shipping.ShippingDate == null
                    || shipping.DeliveryDate == null
                    || order?.DeliveryWarehouseId == null
                    || order?.BodyTypeId == null
                    || string.IsNullOrEmpty(shipping.ShippingWarehouse?.PoolingRegionId))
                {
                    return false;
                }

                var warehouse = _dataService.GetById<Warehouse>(order.DeliveryWarehouseId.Value);
                var bodyType = _dataService.GetById<BodyType>(order.BodyTypeId.Value);
                var company = shipping.CompanyId == null ? null : _dataService.GetById<Company>(shipping.CompanyId.Value);

                if (warehouse == null || bodyType == null)
                {
                    return false;
                }

                var slotFilter = new SlotFilterDto
                {
                    DateFrom = shipping.ShippingDate.Value.ToString("yyyy-MM-dd"),
                    DateTo = shipping.ShippingDate.Value.ToString("yyyy-MM-dd"),
                    DeliveryDateFrom = shipping.DeliveryDate.Value.ToString("yyyy-MM-dd"),
                    DeliveryDateTo = shipping.DeliveryDate.Value.ToString("yyyy-MM-dd"),
                    ShippingRegionId = shipping.ShippingWarehouse.PoolingRegionId,
                    CarType = bodyType.PoolingId,
                    ProductType = (company?.PoolingProductType ?? default).ToString(),
                    OnlyAvailable = true
                };

                if (string.IsNullOrEmpty(warehouse.PoolingId))
                {
                    slotFilter.ClientForeignId = warehouse.Id.ToString();
                }
                else
                {
                    slotFilter.ClientId = warehouse.PoolingId;
                }

                if (string.IsNullOrEmpty(warehouse.DistributionCenterId))
                {
                    slotFilter.UnloadingWarehouseForeignId = warehouse.Id.ToString();
                }
                else
                {
                    slotFilter.UnloadingWarehouseId = warehouse.DistributionCenterId;
                }

                var slots = _poolingApiService.GetSlots(slotFilter, company);
                return slots?.Result != null && slots.Result.Any();
            }
        }

        /// <summary>
        /// Заполнение стоимости по маршруту
        /// </summary>
        private void FillShippingRouteCost(ShippingRoute route, List<Tariff> tariffs, List<AutogroupingType> types)
        {
            var lang = _userProvider.GetCurrentUser()?.Language;

            route.FtlCost = new CostData();

            if (types?.Count > 0 && !types.Contains((int)AutogroupingType.FtlRoute))
            {
                route.FtlCost.Message = "autogrouping.TypeDisabled".Translate(lang);
                return;
            }

            var deliveryPointsCount = route.Shippings.Select(x => x.DeliveryWarehouse?.Id).Distinct().Count();
            if (deliveryPointsCount > 1)
            {
                PseudoShipping longestShipping = null;
                decimal? biggestDistance = -1;
                foreach (var shipping in route.Shippings)
                {
                    var distance = _distancesService.FindDistance(shipping.ShippingWarehouse, shipping.ShippingCity, 
                                                                  shipping.DeliveryWarehouse, shipping.DeliveryCity);
                    if (distance == null)
                    {
                        longestShipping = null;
                        break;
                    }

                    if (distance > biggestDistance)
                    {
                        biggestDistance = distance;
                        longestShipping = shipping;
                    }
                }

                if (longestShipping != null)
                {
                    var palletsCount = (int)Math.Ceiling(route.PalletsCount);
                    var routeTariffs = FindBestTariffs(longestShipping, TarifficationType.Ftl, route.VehicleType, tariffs, deliveryPointsCount);
                    var bestRouteTariff = routeTariffs.FirstOrDefault();
                    if (bestRouteTariff == null)
                    {
                        route.FtlCost.Value = 0M;
                        route.FtlCost.Message = "autogrouping.TariffNotFound".Translate(lang);
                    }
                    else
                    {
                        route.FtlCost.Value = CalculateTariff(bestRouteTariff, TarifficationType.Ftl, longestShipping.ShippingDate, palletsCount)
                                                + (bestRouteTariff?.ExtraPointRate ?? 0M) * (deliveryPointsCount - 1);
                        route.FtlCost.CarrierId = bestRouteTariff.CarrierId;
                        route.FtlCost.IsValid = true;
                    }

                    foreach (var tariff in routeTariffs)
                    {
                        var tariffCost = CalculateTariff(tariff, TarifficationType.Ftl, longestShipping.ShippingDate, palletsCount)
                                                + (tariff?.ExtraPointRate ?? 0M) * (deliveryPointsCount - 1);
                        if (tariffCost != null)
                        {
                            route.AllFtlCosts.Add(new CostData
                            {
                                CarrierId = tariff.CarrierId,
                                Value = tariffCost
                            });
                        }
                    }
                }
                else
                {
                    route.FtlCost.Message = "autogrouping.NoDistances".Translate(lang);
                }
            }
            else
            {
                route.FtlCost.Message = "autogrouping.SingleDeliveryPoint".Translate(lang);
            }
        }

        /// <summary>
        /// Поиск всех подходящих тарифов для псевдоперевозки по указанному способу тарификации
        /// </summary>
        private List<Tariff> FindAllValidTariffs(PseudoShipping shipping, TarifficationType tarifficationType, VehicleType vehicleType, List<Tariff> tariffs)
        {
            tariffs = tariffs.Where(x => x.TarifficationType == tarifficationType
                                        && x.EffectiveDate <= shipping.ShippingDate
                                        && x.ExpirationDate >= shipping.ShippingDate)
                             .ToList();

            var firstOrder = shipping.Orders.First();

            var result = new List<Tariff>();
            if (firstOrder.ShippingWarehouseId != null && firstOrder.DeliveryWarehouseId != null)
            {
                result = tariffs.Where(x => x.ShippingWarehouseId == firstOrder.ShippingWarehouseId
                                            && x.DeliveryWarehouseId == firstOrder.DeliveryWarehouseId)
                                .ToList();
                result = FilterByVehicleType(result, vehicleType);
            }

            if (!result.Any())
            {
                result = tariffs.Where(x => x.ShipmentCity == firstOrder.ShippingCity
                                            && x.DeliveryCity == firstOrder.DeliveryCity
                                            && x.ShippingWarehouseId == null
                                            && x.DeliveryWarehouseId == null)
                                .ToList();
                result = FilterByVehicleType(result, vehicleType);
            }

            if (!result.Any())
            {
                result = tariffs.Where(x => x.ShipmentRegion == firstOrder.ShippingRegion
                                            && x.DeliveryRegion == firstOrder.DeliveryRegion
                                            && string.IsNullOrEmpty(x.ShipmentCity)
                                            && string.IsNullOrEmpty(x.DeliveryCity)
                                            && x.ShippingWarehouseId == null
                                            && x.DeliveryWarehouseId == null)
                                .ToList();
                result = FilterByVehicleType(result, vehicleType);
            }

            return result;
        }

        private List<Tariff> FilterByVehicleType(List<Tariff> tariffs, VehicleType vehicleType)
        {
            if (vehicleType != null && tariffs.Any(x => x.VehicleTypeId == vehicleType.Id))
            {
                tariffs = tariffs.Where(x => x.VehicleTypeId == vehicleType.Id).ToList();
            }
            else
            {
                tariffs = tariffs.Where(x => x.VehicleTypeId == null).ToList();
            }

            if (vehicleType != null && tariffs.Any(x => x.BodyTypeId == vehicleType.BodyTypeId))
            {
                tariffs = tariffs.Where(x => x.BodyTypeId == vehicleType.BodyTypeId).ToList();
            }
            else
            {
                tariffs = tariffs.Where(x => x.BodyTypeId == null).ToList();
            }

            return tariffs;
        }

        /// <summary>
        /// Поиск самого оптимального тарифа для псевдоперевозки по указанному способу тарификации
        /// </summary>
        private List<Tariff> FindBestTariffs(
            PseudoShipping shipping, 
            TarifficationType tarifficationType, 
            VehicleType vehicleType, 
            List<Tariff> allTariffs, 
            int deliveryPointsCount)
        {
            var tariffs = FindAllValidTariffs(shipping, tarifficationType, vehicleType, allTariffs);

            if (tarifficationType == TarifficationType.Ftl)
            {
                return tariffs.Where(x => x.FtlRate != null)
                              .OrderBy(x => x.FtlRate * GetWinterCoeffecient(x, shipping.ShippingDate) + (x.ExtraPointRate ?? 0M) * (deliveryPointsCount - 1))
                              .GroupBy(x => x.CarrierId)
                              .Select(g => g.First())
                              .ToList();
            }
            else
            {
                int palletsCount = Math.Max(1, Math.Min((int)Math.Ceiling(shipping.PalletsCount), 33));

                string propertyName = nameof(Tariff.LtlRate33).Replace("33", palletsCount.ToString());
                var property = typeof(Tariff).GetProperty(propertyName);

                return tariffs.Where(x => property.GetValue(x) != null)
                              .OrderBy(x => (decimal?)property.GetValue(x) * GetWinterCoeffecient(x, shipping.ShippingDate))
                              .GroupBy(x => x.CarrierId)
                              .Select(g => g.First())
                              .ToList();
            }
        }

        private decimal GetWinterCoeffecient(Tariff tariff, DateTime? shippingDate)
        {
            bool needWinterCoeff = tariff.StartWinterPeriod != null
                                && tariff.EndWinterPeriod != null
                                && shippingDate != null
                                && shippingDate >= tariff.StartWinterPeriod
                                && shippingDate <= tariff.EndWinterPeriod
                                && tariff.WinterAllowance != null;
            if (needWinterCoeff)
            {
                return 1 + tariff.WinterAllowance.Value / 100;
            }
            else
            {
                return 1M;
            }
        }

        /// <summary>
        /// Расчет тарифа по указанному числу паллет
        /// </summary>
        private decimal? CalculateTariff(Tariff tariff, TarifficationType tarifficationType, DateTime? shippingDate, int palletsCount)
        {
            if (tariff == null)
            {
                return null;
            }

            decimal? result;
            if (tarifficationType == TarifficationType.Ftl || tarifficationType == TarifficationType.Doubledeck)
            {
                result = tariff.FtlRate;
            }
            else if (palletsCount < 1)
            {
                result = 0M;
            }
            else if (palletsCount < 33)
            {
                string propertyName = nameof(Tariff.LtlRate33).Replace("33", palletsCount.ToString());
                var property = typeof(Tariff).GetProperty(propertyName);
                return (decimal?)property.GetValue(tariff);
            }
            else
            {
                result = tariff.LtlRate33;
            }

            result *= GetWinterCoeffecient(tariff, shippingDate);

            return result;
        }

        private AutogroupingType GetDirectAutogroupingType(TarifficationType type)
        {
            switch(type)
            {
                case TarifficationType.Ftl: return AutogroupingType.FtlDirect;
                case TarifficationType.Ltl: return AutogroupingType.Ltl;
                case TarifficationType.Pooling: return AutogroupingType.Pooling;
                case TarifficationType.Milkrun: return AutogroupingType.Milkrun;
                default: return default;
            }
        }
    }
}
