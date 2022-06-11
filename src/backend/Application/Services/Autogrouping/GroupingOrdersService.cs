using Application.Shared.BodyTypes;
using Application.Shared.Distances;
using Application.Shared.Shippings;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Autogrouping;
using Domain.Services.Translations;
using Domain.Shared;
using Domain.Shared.UserProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Autogrouping
{
    public class GroupingOrdersService : IGroupingOrdersService
    {
        private readonly ICommonDataService _dataService;
        private readonly IUserProvider _userProvider;
        private readonly IDefaultBodyTypeService _bodyTypeService;
        private readonly IGroupCostCalculationService _costCalculationService;
        private readonly IWarehouseDistancesService _warehouseDistancesService;

        private List<VehicleType> _vehicleTypesCache = null;

        public GroupingOrdersService(
            ICommonDataService dataService,
            IUserProvider userProvider,
            IDefaultBodyTypeService bodyTypeService,
            IGroupCostCalculationService costCalculationService,
            IWarehouseDistancesService warehouseDistancesService)
        {
            _dataService = dataService;
            _userProvider = userProvider;
            _bodyTypeService = bodyTypeService;
            _costCalculationService = costCalculationService;
            _warehouseDistancesService = warehouseDistancesService;
        }

        /// <summary>
        /// Группировка накладных по оптимальным перевозкам
        /// </summary>
        public AutogroupingResultData GroupOrders(IEnumerable<IAutogroupingOrder> orders, Guid runId, List<AutogroupingType> types)
        {
            Dictionary<IAutogroupingOrder, string> skippedOrders;
            var result = GroupOrdersInner(orders, runId, types, out skippedOrders);

            if (skippedOrders.Any())
            {
                var lang = _userProvider.GetCurrentUser()?.Language;

                var shippingEntity = new AutogroupingShipping
                {
                    Id = Guid.NewGuid(),
                    RunId = runId,
                    ShippingNumber = "autogrouping.UngroupedOrders".Translate(lang),
                    OrdersCount = skippedOrders.Count,
                    CreatedAt = DateTime.Now
                };
                result.Shippings.Add(shippingEntity);

                foreach (var order in skippedOrders.OrderBy(x => x.Key.OrderNumber))
                {
                    var entry = InitOrderResult(order.Key, runId, shippingEntity.Id);
                    entry.Errors = order.Value;
                    result.Orders.Add(entry);
                }
            }

            return result;
        }

        /// <summary>
        /// Группировка накладных по оптимальным перевозкам без создания перевозки под пропуски
        /// </summary>
        public AutogroupingResultData GroupOrders(
            IEnumerable<IAutogroupingOrder> orders, 
            Guid runId, 
            List<AutogroupingType> types,
            out Dictionary<IAutogroupingOrder, string> skippedOrders)
        {
            return GroupOrdersInner(orders, runId, types, out skippedOrders);
        }

        public ValidateResult MoveOrders(IEnumerable<AutogroupingOrder> orders, AutogroupingShipping targetShipping, List<AutogroupingType> types)
        {
            var result = ValidateMoveOrders(targetShipping, orders);
            if (result.IsError)
            {
                return result;
            }

            List<AutogroupingShipping> shippings;
            Dictionary<Guid?, List<AutogroupingOrder>> shippingOrdersDict;
            LoadMoveOrdersData(orders, targetShipping, out shippings, out shippingOrdersDict);

            foreach (var shipping in shippings)
            {
                if (shippingOrdersDict.TryGetValue(shipping.Id, out List<AutogroupingOrder> shippingOrders))
                {
                    var sourceOrders = shippingOrders.Select(x => x.Order).ToList();
                    var pseudoShippings = GroupPseudoShippings(sourceOrders);
                    var route = CreateRouteForShippings(pseudoShippings);
                    var routes = new[] { route };

                    FitVehicleTypes(routes);
                    _costCalculationService.FillCosts(routes, types);

                    UpdateAutogroupingShippingOrders(shippingOrders, shipping, route);
                    UpdateAutogroupingShipping(shipping, sourceOrders, route);

                    var costs = FillCosts(shipping, route);
                    _dataService.GetDbSet<AutogroupingCost>().AddRange(costs);
                }
                else
                {
                    _dataService.GetDbSet<AutogroupingShipping>().Remove(shipping);
                }
            }

            _dataService.SaveChanges();

            return result;
        }

        private ValidateResult ValidateMoveOrders(AutogroupingShipping targetShipping, IEnumerable<AutogroupingOrder> orders)
        {
            var errors = new List<string>();
            var lang = _userProvider.GetCurrentUser()?.Language;

            if (orders.Any(x => targetShipping.BodyTypeId != x.BodyTypeId))
            {
                errors.Add("Autogrouping.MoveOrders.MismatchBodyType".Translate(lang));
            }

            if (orders.Any(x => targetShipping.ShippingDate?.Date != x.ShippingDate?.Date))
            {
                errors.Add("Autogrouping.MoveOrders.MismatchShippingDate".Translate(lang));
            }

            var orderIds = orders.Select(x => x.Id).ToList();
            var shippingAddresses = _dataService.GetDbSet<AutogroupingOrder>()
                                                .Include(x => x.Order)
                                                .Where(x => x.AutogroupingShippingId == targetShipping.Id || orderIds.Contains(x.Id))
                                                .Select(x => x.Order.ShippingAddress)
                                                .Distinct()
                                                .ToList();
            if (shippingAddresses.Count > 1)
            {
                errors.Add("Autogrouping.MoveOrders.MismatchShippingAddress".Translate(lang));
            }

            return new ValidateResult(string.Join(' ', errors), errors.Count > 0);
        }

        private void LoadMoveOrdersData(
            IEnumerable<AutogroupingOrder> orders, 
            AutogroupingShipping targetShipping, 
            out List<AutogroupingShipping> shippings, 
            out Dictionary<Guid?, List<AutogroupingOrder>> shippingOrdersDict)
        {
            var shippingIds = orders.Select(x => x.AutogroupingShippingId).Concat(new[] { targetShipping?.Id }).Distinct().ToList();
            shippings = _dataService.GetDbSet<AutogroupingShipping>().Where(x => shippingIds.Contains(x.Id)).ToList();

            var costDbSet = _dataService.GetDbSet<AutogroupingCost>();
            var obsoleteCosts = costDbSet.Where(x => shippingIds.Contains(x.AutogroupingShippingId)).ToArray();
            costDbSet.RemoveRange(obsoleteCosts);

            var orderIds = orders.Select(x => x.Id).ToList();
            shippingOrdersDict = _dataService.GetDbSet<AutogroupingOrder>()
                                    .Include(x => x.Order)
                                    .Include(x => x.Order.ShippingWarehouse)
                                    .Include(x => x.Order.DeliveryWarehouse)
                                    .Where(x => x.RunId == targetShipping.RunId
                                            && shippingIds.Contains(x.AutogroupingShippingId)
                                            && !orderIds.Contains(x.Id))
                                    .GroupBy(x => x.AutogroupingShippingId)
                                    .ToDictionary(x => x.Key, x => x.ToList());

            if (!shippingOrdersDict.ContainsKey(targetShipping.Id))
            {
                shippingOrdersDict[targetShipping.Id] = new List<AutogroupingOrder>();
            }
            shippingOrdersDict[targetShipping.Id].AddRange(orders);
        }

        private AutogroupingResultData GroupOrdersInner(
            IEnumerable<IAutogroupingOrder> orders,
            Guid runId,
            List<AutogroupingType> types,
            out Dictionary<IAutogroupingOrder, string> skippedOrders)
        {
            var result = new AutogroupingResultData
            {
                Orders = new List<AutogroupingOrder>(),
                Shippings = new List<AutogroupingShipping>(),
                Costs = new List<AutogroupingCost>()
            };
            var lang = _userProvider.GetCurrentUser()?.Language;

            List<IAutogroupingOrder> ordersToGroup = new List<IAutogroupingOrder>();
            skippedOrders = new Dictionary<IAutogroupingOrder, string>();
            foreach (var order in orders)
            {
                var orderVehicleType = FindVehicleType(order);
                if (order.Status != OrderState.Created && order.Status != OrderState.Confirmed)
                {
                    skippedOrders[order] = "autogrouping.InvalidOrderStatus".Translate(lang);
                }
                else if (order.ShippingDate == null
                        || order.DeliveryDate == null
                        || string.IsNullOrEmpty(order.ShippingAddress)
                        || string.IsNullOrEmpty(order.ShippingRegion)
                        || string.IsNullOrEmpty(order.DeliveryAddress)
                        || string.IsNullOrEmpty(order.DeliveryRegion))
                {
                    var fields = new List<string>();
                    if (order.ShippingDate == null) fields.Add(nameof(order.ShippingDate).FormatEnum());
                    if (order.DeliveryDate == null) fields.Add(nameof(order.DeliveryDate).FormatEnum());
                    if (string.IsNullOrEmpty(order.ShippingAddress)) fields.Add(nameof(order.ShippingAddress).FormatEnum());
                    if (string.IsNullOrEmpty(order.ShippingRegion)) fields.Add(nameof(order.ShippingRegion).FormatEnum());
                    if (string.IsNullOrEmpty(order.DeliveryAddress)) fields.Add(nameof(order.DeliveryAddress).FormatEnum());
                    if (string.IsNullOrEmpty(order.DeliveryRegion)) fields.Add(nameof(order.DeliveryRegion).FormatEnum());
                    var fieldNames = string.Join(", ", fields.Select(x => x.Translate(lang)));

                    skippedOrders[order] = "autogrouping.EmptyFields".Translate(lang, fieldNames);
                }
                else if (orderVehicleType == null)
                {
                    skippedOrders[order] = "autogrouping.VehicleTypNotFound".Translate(lang);
                }
                else
                {
                    ordersToGroup.Add(order);
                }
            }

            var shippings = GroupPseudoShippings(ordersToGroup);
            var routes = GroupShippingRoutes(shippings);
            FitVehicleTypes(routes);
            _costCalculationService.FillCosts(routes, types);
            CreateOrderGroups(routes, runId, types, ref result, ref skippedOrders);

            return result;
        }

        private ShippingRoute CreateRouteForShippings(List<PseudoShipping> shippings)
        {
            return new ShippingRoute
            {
                AllFtlCosts = new List<CostData>(),
                Shippings = shippings,
                VehicleType = shippings.FirstOrDefault()?.VehicleType,
                PalletsCount = shippings.Sum(x => x.PalletsCount),
                WeightKg = shippings.Sum(x => x.WeightKg)
            };
        }

        private void UpdateAutogroupingShippingOrders(List<AutogroupingOrder> orders, AutogroupingShipping shipping, ShippingRoute route)
        {
            foreach (var order in orders)
            {
                order.AutogroupingShippingId = shipping.Id;
                order.BodyTypeId = route.VehicleType?.BodyTypeId;
                order.VehicleTypeId = route.VehicleType?.Id;
            }
        }

        private void UpdateAutogroupingShipping(AutogroupingShipping shipping, List<Order> orders, ShippingRoute route)
        {
            var autogroupingType = AutogroupingType.FtlRoute;
            var tarifficationType = TarifficationType.Ftl;
            var carrierId = route.FtlCost?.CarrierId;
            var bestCost = route.FtlCost?.Value;
            if (route.Shippings.Count == 1)
            {
                var routeShipping = route.Shippings.First();
                if (routeShipping.Costs != null && routeShipping.Costs.Any())
                {
                    var orderedEntries = routeShipping.Costs.Where(x => x.Value?.IsValid == true)
                                                            .OrderBy(x => x.Value.Value)
                                                            .ThenByDescending(x => x.Key);
                    if (orderedEntries.Any())
                    {
                        var bestEntry = orderedEntries.First();
                        bestCost = bestEntry.Value?.Value;
                        tarifficationType = bestEntry.Key;
                        autogroupingType = GetAutogroupingTypeByTarification(bestEntry.Key);
                        carrierId = bestEntry.Value?.CarrierId;
                    }
                }
            }

            shipping.Route = MakeRouteName(route.Shippings);
            shipping.AutogroupingType = autogroupingType;
            shipping.TarifficationType = tarifficationType;
            shipping.CarrierId = carrierId;
            shipping.BodyTypeId = route.VehicleType?.BodyTypeId;
            shipping.VehicleTypeId = route.VehicleType?.Id;
            shipping.ShippingDate = orders.Min(y => y.ShippingDate);
            shipping.DeliveryDate = orders.Max(y => y.DeliveryDate);
            shipping.OrdersCount = orders.Count;
            shipping.PalletsCount = (int)Math.Ceiling(orders.Sum(y => y.PalletsCount ?? 0M));
            shipping.WeightKg = orders.Sum(y => y.WeightKg ?? 0M);
            shipping.BestCost = bestCost;
        }

        /// <summary>
        /// Объединение накладных по псевдоперевозкам (точка-точка)
        /// </summary>
        private List<PseudoShipping> GroupPseudoShippings(IEnumerable<IAutogroupingOrder> orders)
        {
            var shippings = new List<PseudoShipping>();
            int lastShippingsCount = 0;
            foreach (var ordersGroup in orders.Select(x => new { Order = x, VehicleType = FindVehicleType(x) })
                                              .GroupBy(x => new
                                              {
                                                  VehicleTypeId = x.VehicleType?.Id,
                                                  x.Order.CompanyId,
                                                  ShippingDate = x.Order.ShippingDate?.Date,
                                                  DeliveryDate = x.Order.DeliveryDate?.Date,
                                                  ShippingAddress = x.Order.ShippingAddress?.Trim(),
                                                  DeliveryAddress = x.Order.DeliveryAddress?.Trim()
                                              })
                                              .OrderBy(x => x.Key.ShippingDate))
            {
                foreach (var order in ordersGroup.OrderByDescending(x => x.Order.PalletsCount))
                {
                    bool isShippingFound = false;
                    foreach (var shipping in shippings.Skip(lastShippingsCount))
                    {
                        if (shipping.PalletsCount + order.Order.PalletsCount > shipping.VehicleType.PalletsCount
                            || shipping.WeightKg + order.Order.WeightKg > shipping.VehicleType.Tonnage.WeightKg)
                        {
                            continue;
                        }

                        shipping.PalletsCount += order.Order.PalletsCount ?? 0M;
                        shipping.WeightKg += order.Order.WeightKg ?? 0M;
                        shipping.Orders.Add(order.Order);
                        isShippingFound = true;
                        break;
                    }

                    if (!isShippingFound)
                    {
                        var shipping = CreateNewPseudoShipping(order.Order, order.VehicleType);
                        shippings.Add(shipping);
                    }
                }
                lastShippingsCount = shippings.Count;
            }
            return shippings;
        }

        /// <summary>
        /// Объединение псевдоперевозок по маршрутам (с общим регионом доставки)
        /// </summary>
        private List<ShippingRoute> GroupShippingRoutes(List<PseudoShipping> shippings)
        {
            var routes = new List<ShippingRoute>();
            int lastRoutesCount = 0;
            foreach (var shippingsGroup in shippings.GroupBy(x => new
            {
                ShippingDate = x.ShippingDate?.Date,
                x.VehicleType?.BodyTypeId,
                ShippingAddress = x.ShippingAddress?.Trim()
            }))
            {
                var companyId = shippingsGroup.First().CompanyId;
                var settings = _dataService.GetDbSet<AutogroupingSetting>()
                                           .FirstOrDefault(x => x.CompanyId == companyId);
                var maxUnloadingPoints = settings?.MaxUnloadingPoints;

                foreach (var shipping in shippingsGroup.OrderByDescending(x => x.RouteDistance).ThenByDescending(x => x.PalletsCount))
                {
                    bool isShippingFound = false;
                    foreach (var route in routes.Skip(lastRoutesCount))
                    {
                        if (route.PalletsCount + shipping.PalletsCount > route.VehicleType.PalletsCount
                            || route.WeightKg + shipping.WeightKg > route.VehicleType.Tonnage.WeightKg)
                        {
                            continue;
                        }

                        if (maxUnloadingPoints != null)
                        {
                            var unloadingPointsCount = route.Shippings.Select(x => x.DeliveryAddress)
                                                                      .Concat(new[] { shipping.DeliveryAddress })
                                                                      .Where(x => x != null)
                                                                      .Distinct()
                                                                      .Count();
                            if (unloadingPointsCount > maxUnloadingPoints)
                            {
                                continue;
                            }
                        }

                        var allShippings = route.Shippings.Concat(new[] { shipping });
                        var overrunCoef = CalculateOverrunCoefficient(allShippings);
                        var isInterregion = allShippings.SelectMany(x => x.Orders.SelectMany(y => new[] { y.ShippingRegion, y.DeliveryRegion }))
                                                        .Distinct()
                                                        .Count() > 1;
                        if (overrunCoef == null
                            || (isInterregion && settings?.InterregionOverrunCoefficient != null && overrunCoef > settings.InterregionOverrunCoefficient)
                            || (!isInterregion && settings?.RegionOverrunCoefficient != null && overrunCoef > settings.RegionOverrunCoefficient))
                        {
                            continue;
                        }

                        route.PalletsCount += shipping.PalletsCount;
                        route.WeightKg += shipping.WeightKg;
                        route.Shippings.Add(shipping);
                        isShippingFound = true;
                        break;
                    }

                    if (!isShippingFound)
                    {
                        var route = CreateNewShippingRoute(shipping);
                        routes.Add(route);
                    }
                }
                lastRoutesCount = routes.Count;
            }
            return routes;
        }

        private decimal? CalculateOverrunCoefficient(IEnumerable<PseudoShipping> shippings)
        {
            var maxDistance = shippings.Max(x => x.RouteDistance);
            if (maxDistance == null || maxDistance == 0)
            {
                return null;
            }

            var points = shippings.SelectMany(x => x.Orders.SelectMany(y => new[] {
                new { Date = y.ShippingDate, Warehouse = (IMapPoint)y.ShippingWarehouse, City = y.ShippingCity, x.RouteDistance },
                new { Date = y.DeliveryDate, Warehouse = (IMapPoint)y.DeliveryWarehouse, City = y.DeliveryCity, x.RouteDistance }
            }));

            IMapPoint lastWarehouse = null;
            string lastCity = null;
            decimal totalDistance = 0M;
            foreach (var point in points.OrderBy(x => x.Date).ThenBy(x => x.RouteDistance))
            {
                if (lastWarehouse?.Id != point.Warehouse?.Id || lastCity != point.City)
                {
                    if (lastWarehouse != null || lastCity != null)
                    {
                        var distance = _warehouseDistancesService.FindDistance(lastWarehouse, lastCity, point.Warehouse, point.City);
                        if (distance == null)
                        {
                            return null;
                        }
                        totalDistance += distance.Value;
                    }

                    lastCity = point.City;
                    lastWarehouse = point.Warehouse;
                }
            }

            return totalDistance / maxDistance.Value;
        }
        
        /// <summary>
        /// Создание новой псевдоперевозки для накладной
        /// </summary>
        private PseudoShipping CreateNewPseudoShipping(IAutogroupingOrder order, VehicleType vehicleType)
        {
            var orderDistance = _warehouseDistancesService.FindDistance(
                order.ShippingWarehouse, order.ShippingCity,
                order.DeliveryWarehouse, order.DeliveryCity);
            var result = new PseudoShipping
            {
                Orders = new List<IAutogroupingOrder> { order },
                Costs = new Dictionary<TarifficationType, CostData>(),
                AllCosts = new Dictionary<AutogroupingType, List<CostData>>(),
                CompanyId = order.CompanyId,
                VehicleType = vehicleType,
                ShippingDate = order.ShippingDate,
                ShippingWarehouse = order.ShippingWarehouse,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                DeliveryDate = order.DeliveryDate,
                DeliveryWarehouse = order.DeliveryWarehouse,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryRegion = MergeSameRegion(order.DeliveryRegion),
                DeliveryCity = order.DeliveryCity,
                RouteDistance = orderDistance,
                PalletsCount = order.PalletsCount ?? 0,
                WeightKg = order.WeightKg ?? 0M
            };
            return result;
        }

        /// <summary>
        /// Созданое нового маршрута для псевдоперевозки
        /// </summary>
        private ShippingRoute CreateNewShippingRoute(PseudoShipping shipping)
        {
            var result = new ShippingRoute
            {
                Shippings = new List<PseudoShipping> { shipping },
                AllFtlCosts = new List<CostData>(),
                VehicleType = shipping.VehicleType,
                PalletsCount = shipping.PalletsCount,
                WeightKg = shipping.WeightKg
            };
            return result;
        }

        private void FitVehicleTypes(IEnumerable<ShippingRoute> routes)
        {
            foreach (var route in routes)
            {
                var totalPallets = (int)Math.Ceiling(route.Shippings.Sum(x => x.Orders.Sum(y => y.PalletsCount ?? 0M)));
                var totalWeight = route.Shippings.Sum(x => x.Orders.Sum(y => y.WeightKg ?? 0M));
                var isInterregion = route.Shippings.SelectMany(x => x.Orders.SelectMany(y => new[] { y.ShippingRegion, y.DeliveryRegion }))
                                                   .Distinct()
                                                   .Count() > 1;
                route.VehicleType = FindOptimalVehicleType(route.VehicleType?.BodyTypeId, totalPallets, totalWeight, isInterregion);
            }

            foreach (var shipping in routes.SelectMany(x => x.Shippings))
            {
                var totalPallets = (int)Math.Ceiling(shipping.Orders.Sum(y => y.PalletsCount ?? 0M));
                var totalWeight = shipping.Orders.Sum(y => y.WeightKg ?? 0M);
                var isInterregion = shipping.Orders.SelectMany(y => new[] { y.ShippingRegion, y.DeliveryRegion }).Distinct().Count() > 1;
                shipping.VehicleType = FindOptimalVehicleType(shipping.VehicleType?.BodyTypeId, totalPallets, totalWeight, isInterregion);
            }
        }

        private VehicleType FindOptimalVehicleType(Guid? bodyTypeId, int totalPallets, decimal totalWeight, bool isInterregion)
        {
            EnsureVehicleTypeCache();

            var result = _vehicleTypesCache.Where(x => x.BodyTypeId == bodyTypeId
                                                    && x.PalletsCount >= totalPallets
                                                    && x.Tonnage.WeightKg >= totalWeight
                                                    && (!isInterregion || x.IsInterregion == true))
                                           .OrderBy(x => x.PalletsCount)
                                           .FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Формирование перевозок по результату расчета стоимостей для маршрутов и псевдоперевозок
        /// </summary>
        private void CreateOrderGroups(IEnumerable<ShippingRoute> routes, Guid runId, List<AutogroupingType> types, 
                                       ref AutogroupingResultData result, ref Dictionary<IAutogroupingOrder, string> skippedOrders)
        {
            int routeNumber = 0;
            foreach (var route in routes)
            {
                var isRoute = route.Shippings.Count > 1;
                if (isRoute)
                {
                    ++routeNumber;
                }
                var orderRouteNumber = isRoute ? routeNumber : (int?)null;

                if (route.FtlCost?.IsValid != true || route.Shippings.Count == 1)
                {
                    CreateShippingOrderGroups(route, runId, orderRouteNumber, types, ref result, ref skippedOrders);
                }
                else
                {
                    var shippingsBestSum = GetBestSum(route.Shippings);
                    if (shippingsBestSum != null && shippingsBestSum < route.FtlCost?.Value)
                    {
                        CreateShippingOrderGroups(route, runId, orderRouteNumber, types, ref result, ref skippedOrders);
                    }
                    else
                    {
                        CreateRouteOrderGroups(route, runId, orderRouteNumber, ref result);
                    }
                }
            }
        }

        /// <summary>
        /// Создание оптимальных перевозок для маршрута
        /// </summary>
        private void CreateRouteOrderGroups(ShippingRoute route, Guid runId, int? routeNumber, ref AutogroupingResultData result)
        {
            var shippingEntity = new AutogroupingShipping
            {
                Id = Guid.NewGuid(),
                RunId = runId,
                ShippingNumber = ShippingNumberProvider.GetNextShippingNumber(),
                Route = MakeRouteName(route.Shippings),
                RouteNumber = routeNumber,
                AutogroupingType = AutogroupingType.FtlRoute,
                TarifficationType = TarifficationType.Ftl,
                CarrierId = route.FtlCost?.CarrierId,
                BodyTypeId = route.VehicleType?.BodyTypeId,
                VehicleTypeId = route.VehicleType?.Id,
                ShippingDate = route.Shippings.Min(x => x.Orders.Min(y => y.ShippingDate)),
                DeliveryDate = route.Shippings.Max(x => x.Orders.Max(y => y.DeliveryDate)),
                OrdersCount = route.Shippings.Sum(x => x.Orders.Count),
                PalletsCount = (int)Math.Ceiling(route.Shippings.Sum(x => x.Orders.Sum(y => y.PalletsCount ?? 0M))),
                WeightKg = route.Shippings.Sum(x => x.Orders.Sum(y => y.WeightKg ?? 0M)),
                BestCost = route.FtlCost?.Value,
                CreatedAt = DateTime.Now
            };
            var costs = FillCosts(shippingEntity, route);
            result.Shippings.Add(shippingEntity);
            result.Costs.AddRange(costs);

            foreach (var shipping in route.Shippings)
            {
                foreach (var order in shipping.Orders)
                {
                    var orderEntry = InitOrderResult(order, runId, shippingEntity.Id);
                    orderEntry.BodyTypeId = route.VehicleType?.BodyTypeId;
                    orderEntry.VehicleTypeId = route.VehicleType?.Id;
                    result.Orders.Add(orderEntry);
                }
            }
        }

        /// <summary>
        /// Создание оптимальных перевозок для псевдоперевозок
        /// </summary>
        private void CreateShippingOrderGroups(ShippingRoute route, Guid runId, int? routeNumber, List<AutogroupingType> types, 
                                               ref AutogroupingResultData result, ref Dictionary<IAutogroupingOrder, string> skippedOrders)
        {
            foreach (var shipping in route.Shippings)
            {
                decimal? bestCost = null;
                AutogroupingType? autogroupingType = null;
                TarifficationType? tarifficationType = null;
                Guid? carrierId = null;
                if (shipping.Costs != null && shipping.Costs.Any())
                {
                    var orderedEntries = shipping.Costs.Where(x => x.Value?.IsValid == true)
                                                       .OrderBy(x => x.Value.Value)
                                                       .ThenByDescending(x => x.Key);
                    if (orderedEntries.Any())
                    {
                        var bestEntry = orderedEntries.First();
                        bestCost = bestEntry.Value?.Value;
                        tarifficationType = bestEntry.Key;
                        autogroupingType = GetAutogroupingTypeByTarification(bestEntry.Key);
                        carrierId = bestEntry.Value?.CarrierId;
                    }
                }

                if (autogroupingType == null)
                {
                    var lang = _userProvider.GetCurrentUser()?.Language;

                    var costErrors = new List<string>();
                    if (shipping.Costs != null)
                    {
                        foreach (var costData in shipping.Costs)
                        {
                            if (types?.Count > 0 && !types.Contains(GetAutogroupingTypeByTarification(costData.Key)))
                            {
                                continue;
                            }

                            if (!string.IsNullOrEmpty(costData.Value.Message))
                            {
                                costErrors.Add($"{costData.Key.FormatEnum().Translate(lang)}: {costData.Value.Message}");
                            }
                        }
                    }

                    string skipMessage = null;
                    if (costErrors.Any())
                    {
                        skipMessage = string.Join(". ", costErrors);
                    }
                    else
                    {
                        skipMessage = "autogrouping.TariffNotFound".Translate(lang);
                    }

                    foreach (var order in shipping.Orders)
                    {
                        skippedOrders[order] = skipMessage;
                    }
                }
                else
                {
                    var shippingEntity = new AutogroupingShipping
                    {
                        Id = Guid.NewGuid(),
                        RunId = runId,
                        ShippingNumber = ShippingNumberProvider.GetNextShippingNumber(),
                        Route = MakeRouteName(new[] { shipping }),
                        RouteNumber = routeNumber,
                        AutogroupingType = autogroupingType,
                        TarifficationType = tarifficationType,
                        CarrierId = carrierId,
                        BodyTypeId = shipping.VehicleType?.BodyTypeId,
                        VehicleTypeId = shipping.VehicleType?.Id,
                        ShippingDate = shipping.Orders.Min(y => y.ShippingDate),
                        DeliveryDate = shipping.Orders.Max(y => y.DeliveryDate),
                        OrdersCount = shipping.Orders.Count,
                        PalletsCount = (int)Math.Ceiling(shipping.Orders.Sum(y => y.PalletsCount ?? 0M)),
                        WeightKg = shipping.Orders.Sum(y => y.WeightKg ?? 0M),
                        BestCost = bestCost,
                        CreatedAt = DateTime.Now
                    };
                    var costs = FillCosts(shippingEntity, shipping, route);
                    result.Shippings.Add(shippingEntity);
                    result.Costs.AddRange(costs);

                    foreach (var order in shipping.Orders)
                    {
                        var resultEntry = InitOrderResult(order, runId, shippingEntity.Id);
                        resultEntry.BodyTypeId = shipping.VehicleType?.BodyTypeId;
                        resultEntry.VehicleTypeId = shipping.VehicleType?.Id;
                        result.Orders.Add(resultEntry);
                    }
                }
            }
        }

        /// <summary>
        /// Заполнение вариантов стоимости для накладной по результатам расчета для маршрута и псевдоперевозки
        /// </summary>
        private List<AutogroupingCost> FillCosts(AutogroupingShipping entry, PseudoShipping shipping, ShippingRoute route)
        {
            entry.FtlRouteCost = route.FtlCost?.Value;
            entry.FtlRouteCostMessage = route.FtlCost?.Message;

            var ftlCost = GetShippingCost(shipping, TarifficationType.Ftl);
            entry.FtlDirectCost = ftlCost?.Value;
            entry.FtlDirectCostMessage = ftlCost?.Message;

            var ltlCost = GetShippingCost(shipping, TarifficationType.Ltl);
            entry.LtlCost = ltlCost?.Value;
            entry.LtlCostMessage = ltlCost?.Message;

            var poolingCost = GetShippingCost(shipping, TarifficationType.Pooling);
            entry.PoolingCost = poolingCost?.Value;
            entry.PoolingCostMessage = poolingCost?.Message;

            var milkrunCost = GetShippingCost(shipping, TarifficationType.Milkrun);
            entry.MilkrunCost = milkrunCost?.Value;
            entry.MilkrunCostMessage = milkrunCost?.Message;

            var result = new List<AutogroupingCost>();
            foreach (var ftlCostValue in route.AllFtlCosts)
            {
                result.Add(CreateAutogroupingCost(entry, AutogroupingType.FtlRoute, ftlCostValue));
            }
            foreach (var typeCosts in shipping.AllCosts)
            {
                foreach (var costValue in typeCosts.Value)
                {
                    result.Add(CreateAutogroupingCost(entry, typeCosts.Key, costValue));
                }
            }
            return result;
        }

        /// <summary>
        /// Заполнение вариантов стоимости для накладной по результатам расчета для маршрута
        /// </summary>
        private List<AutogroupingCost> FillCosts(AutogroupingShipping entry, ShippingRoute route)
        {
            entry.FtlRouteCost = route.FtlCost?.Value;
            entry.FtlRouteCostMessage = route.FtlCost?.Message;

            var ftlCosts = route.Shippings.Select(x => GetShippingCost(x, TarifficationType.Ftl)).ToList();
            var ftlAllCosts = ftlCosts.All(x => x != null && x.IsValid && x.Value != null);
            entry.FtlDirectCost = ftlAllCosts ? ftlCosts.Sum(x => x.Value ?? 0M) : (decimal?)null;
            entry.FtlDirectCostMessage = ftlCosts.Select(x => x.Message).Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault();

            var ltlCosts = route.Shippings.Select(x => GetShippingCost(x, TarifficationType.Ltl)).ToList();
            var ltlAllCosts = ltlCosts.All(x => x != null && x.IsValid && x.Value != null);
            entry.LtlCost = ltlAllCosts ? ltlCosts.Sum(x => x.Value ?? 0M) : (decimal?)null;
            entry.LtlCostMessage = ltlCosts.Select(x => x.Message).Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault();

            var poolingCosts = route.Shippings.Select(x => GetShippingCost(x, TarifficationType.Pooling)).ToList();
            var poolingAllCosts = poolingCosts.All(x => x != null && x.IsValid && x.Value != null);
            entry.PoolingCost = poolingAllCosts ? poolingCosts.Sum(x => x.Value ?? 0M) : (decimal?)null;
            entry.PoolingCostMessage = poolingCosts.Select(x => x.Message).Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault();

            var milkrunCosts = route.Shippings.Select(x => GetShippingCost(x, TarifficationType.Milkrun)).ToList();
            var milkrunAllCosts = milkrunCosts.All(x => x != null && x.IsValid && x.Value != null);
            entry.MilkrunCost = milkrunAllCosts ? milkrunCosts.Sum(x => x.Value ?? 0M) : (decimal?)null;
            entry.MilkrunCostMessage = milkrunCosts.Select(x => x.Message).Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault();

            var result = new List<AutogroupingCost>();
            foreach (var ftlCostValue in route.AllFtlCosts)
            {
                result.Add(CreateAutogroupingCost(entry, AutogroupingType.FtlRoute, ftlCostValue));
            }
            if (route.Shippings.Count == 1)
            {
                foreach (var typeCosts in route.Shippings.First().AllCosts)
                {
                    foreach (var costValue in typeCosts.Value)
                    {
                        result.Add(CreateAutogroupingCost(entry, typeCosts.Key, costValue));
                    }
                }
            }
            return result;
        }


        private AutogroupingCost CreateAutogroupingCost(AutogroupingShipping shipping, AutogroupingType autogroupingType, CostData costValue)
        {
            return new AutogroupingCost
            {
                Id = Guid.NewGuid(),
                AutogroupingShippingId = shipping.Id,
                CarrierId = costValue.CarrierId.Value,
                AutogroupingType = autogroupingType,
                Value = costValue.Value,
                CreatedAt = shipping.CreatedAt
            };
        }

        /// <summary>
        /// Получение способа доставки по способу тарификации псевдоперевозки
        /// </summary>
        private AutogroupingType GetAutogroupingTypeByTarification(TarifficationType tarifficationType)
        {
            switch (tarifficationType)
            {
                case TarifficationType.Ftl: return AutogroupingType.FtlDirect;
                case TarifficationType.Ltl: return AutogroupingType.Ltl;
                case TarifficationType.Pooling: return AutogroupingType.Pooling;
                case TarifficationType.Milkrun: return AutogroupingType.Milkrun;
                default: return default;
            }
        }

        /// <summary>
        /// Получение стоимости псевдоперевозки по указанному способу тарификации
        /// </summary>
        private CostData GetShippingCost(PseudoShipping shipping, TarifficationType tarifficationType)
        {
            if (shipping.Costs == null || !shipping.Costs.ContainsKey(tarifficationType))
            {
                return null;
            }
            return shipping.Costs[tarifficationType];
        }

        /// <summary>
        /// Поиск оптимальной стоимости доставки для всех псевдоперевозок, входящих в маршрут
        /// </summary>
        private decimal? GetBestSum(IEnumerable<PseudoShipping> shippings)
        {
            decimal? result = null;
            foreach (var shipping in shippings)
            {
                if (shipping.Costs == null || !shipping.Costs.Any(x => x.Value?.IsValid == true))
                {
                    return null;
                }
                result = (result ?? 0M) + shipping.Costs.Values.Where(x => x?.IsValid == true)
                                                               .Select(x => x.Value)
                                                               .Min();
            }
            return result;
        }

        /// <summary>
        /// Создание результата расчета для накладной
        /// </summary>
        private AutogroupingOrder InitOrderResult(IAutogroupingOrder order, Guid runId, Guid? groupShippingId)
        {
            var result = new AutogroupingOrder
            {
                Id = Guid.NewGuid(),
                RunId = runId,
                AutogroupingShippingId = groupShippingId,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                ShippingDate = order.ShippingDate,
                ShippingWarehouseId = order.ShippingWarehouseId,
                DeliveryDate = order.DeliveryDate?.Date,
                DeliveryRegion = order.DeliveryRegion,
                DeliveryTime = order.DeliveryDate?.TimeOfDay,
                DeliveryWarehouseId = order.DeliveryWarehouseId,
                PalletsCount = order.PalletsCount,
                WeightKg = order.WeightKg,
                BodyTypeId = order.BodyTypeId,
                VehicleTypeId = order.VehicleTypeId,
                CreatedAt = DateTime.Now
            };
            return result;
        }

        /// <summary>
        /// Формирование названия маршрута
        /// </summary>
        private string MakeRouteName(IEnumerable<PseudoShipping> shippings)
        {
            var shippingPoint = shippings.SelectMany(x => x.Orders)
                                         .Select(x => x.ShippingWarehouse == null ? x.ShippingCity : x.ShippingWarehouse.WarehouseName)
                                         .FirstOrDefault();

            var deliveryPoints = shippings.SelectMany(x => x.Orders.Select(y => new { y.DeliveryDate, y.DeliveryWarehouse, y.DeliveryCity, x.RouteDistance }))
                                          .OrderBy(x => x.DeliveryDate)
                                          .ThenBy(x => x.RouteDistance)
                                          .Select(x => x.DeliveryWarehouse == null ? x.DeliveryCity : x.DeliveryWarehouse.WarehouseName)
                                          .Where(x => !string.IsNullOrEmpty(x));

            var uniqueDeliveryPoints = new List<string>();
            foreach (var point in deliveryPoints)
            {
                if (!uniqueDeliveryPoints.Any() || uniqueDeliveryPoints.Last() != point)
                {
                    uniqueDeliveryPoints.Add(point);
                }
            }

            return $"{shippingPoint} - {string.Join(" - ", uniqueDeliveryPoints)}";
        }

        /// <summary>
        /// Объединение одинаковых регионов
        /// </summary>
        private string MergeSameRegion(string region)
        {
            if (!string.IsNullOrEmpty(region) &&
                (region.ToLower().Contains("москва") || region.ToLower().Contains("московская")))
            {
                return "Москва";
            }
            else
            {
                return region;
            }
        }

        /// <summary>
        /// Поиск Типа ТС под псевдоперевозку
        /// </summary>
        private VehicleType FindVehicleType(IAutogroupingOrder order)
        {
            EnsureVehicleTypeCache();

            var settings = _dataService.GetDbSet<AutogroupingSetting>()
                                       .FirstOrDefault(x => x.CompanyId == order.CompanyId);

            var tonnageId = settings?.TonnageId;
            if (tonnageId == null)
            {
                tonnageId = _dataService.GetDbSet<Tonnage>()
                                        .Where(x => x.CompanyId == order.CompanyId)
                                        .OrderByDescending(x => x.WeightKg)
                                        .FirstOrDefault()?
                                        .Id;
            }

            var bodyTypeId = order.BodyTypeId;
            if (bodyTypeId == null)
            {
                bodyTypeId = _bodyTypeService.GetDefaultBodyType(
                                                order.ShippingDate, 
                                                order.ShippingWarehouseId, 
                                                order.DeliveryWarehouseId,
                                                order.ShippingCity,
                                                order.DeliveryCity,
                                                order.ShippingRegion,
                                                order.DeliveryRegion,
                                                order.CompanyId)?.Id;
            }
            if (bodyTypeId == null)
            {
                bodyTypeId = _dataService.GetDbSet<BodyType>()
                                         .FirstOrDefault(x => x.CompanyId == order.CompanyId)?
                                         .Id;
            }

            return _vehicleTypesCache.Where(x => x.BodyTypeId == bodyTypeId && x.TonnageId == tonnageId)
                                     .OrderByDescending(x => x.PalletsCount)
                                     .FirstOrDefault();
        }

        private void EnsureVehicleTypeCache()
        {
            if (_vehicleTypesCache == null)
            {
                _vehicleTypesCache = _dataService.GetDbSet<VehicleType>()
                                                 .Include(x => x.Tonnage)
                                                 .ToList();
            }
        }
    }
}
