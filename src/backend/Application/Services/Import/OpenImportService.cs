using Application.Services.Import.ImportObject;
using Application.Shared.BodyTypes;
using Application.Shared.Shippings;
using Application.Shared.Triggers;
using AutoMapper;
using DAL.Services;
using Domain.Persistables;
using Domain.Services.Import;
using Domain.Shared.UserProvider;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Import
{
    public class OpenImportService : IOpenImportService
    {
        private readonly IMapper _mapper;
        private readonly IUserProvider _userProvider;
        private readonly ICommonDataService _dataService;
        private readonly IShippingActionService _shippingActionService;
        private readonly IDefaultBodyTypeService _defaultBodyTypeService;
        private readonly ITriggersService _triggersService;

        public OpenImportService(IUserProvider userProvider, ICommonDataService dataService, IShippingActionService shippingActionService, IDefaultBodyTypeService defaultBodyTypeService, ITriggersService triggersService)
        {
            _defaultBodyTypeService = defaultBodyTypeService;
            _shippingActionService = shippingActionService;
            _userProvider = userProvider;
            _dataService = dataService;
            _triggersService = triggersService;
            _mapper = new MapperConfiguration(cgf=>cgf.CreateMap<Consignee,Warehouse>()).CreateMapper();
        }

        public void ImportShippings(string requestData)
        {
            var requestDto = ParseRequest(requestData);
            RegisterInboundFile(requestData, requestDto);
            var orderItems = MapRequestToOrder(requestDto);
            CreateEntities(orderItems);
            _triggersService.Execute(false);
            _dataService.SaveChanges();
        }

        private List<RouteItem> ParseRequest(string requestData)
        {
            var parsedRequest = JObject.Parse(requestData).SelectToken("DATA.ROUTE_ITEMS").ToString();
            var result = JsonConvert.DeserializeObject<List<RouteItem>>(parsedRequest);
            return result;
        }

        private void CreateEntities(List<Order> orders)
        {
            _dataService.GetDbSet<Order>().AddRange(orders);
            var groupedOrders = orders.GroupBy(x => x.ShippingNumber);
            foreach (var item in groupedOrders)
            {
                var bodyType = _defaultBodyTypeService.GetDefaultBodyType(item.ToList().First());

                foreach (var order in orders)
                    order.BodyTypeId = bodyType.Id;

                _shippingActionService.UnionOrders(item.ToList());
            }
        }

        private List<Order> MapRequestToOrder(List<RouteItem> routeItems)
        {
            var result = new List<Order>();
            var warehouses = _dataService.GetDbSet<Warehouse>();
            var orderItems = _dataService.GetDbSet<OrderItem>();
            foreach (var routeItem in routeItems)
            {
                var shippingHouse = _dataService.GetDbSet<ShippingWarehouse>().FirstOrDefault(x => x.Code == routeItem.Code);

                foreach (var transItem in routeItem.TransItems)
                {
                    foreach (var delivery in transItem.Deliveries)
                    {
                        Warehouse warehouse = warehouses.FirstOrDefault(x => x.Client == delivery.Consignee.Client && x.Address == delivery.Consignee.Address);
                        if (warehouse == null)
                        {
                            warehouse = _mapper.Map<Warehouse>(delivery.Consignee);
                            warehouse.Id = Guid.NewGuid();
                            warehouses.Add(warehouse);
                        }
                        var newOrder = new Order()
                        {
                            Id = Guid.NewGuid(),
                            TransportZone = routeItem.TransportZone,
                            ShippingNumber = transItem.ShippingNumber,
                            ShippingDate = DateTime.Parse(delivery.ShippingDate),
                            WeightKg = ToDecimal(delivery.WeightKg),
                            Volume9l = ToDecimal(delivery.Volume9l),
                            PaymentCondition = delivery.PaymentCondition,
                            OrderNumber = delivery.InvoiceAmountExcludingVAT,
                            DeviationsComment = delivery.DeviationsComment,
                            OrderAmountExcludingVAT = ToDecimal(delivery.OrderAmountExcludingVAT),
                            BottlesCount = Convert.ToInt32(ToDecimal(delivery.BottlesCount)),
                            ShippingWarehouseId = shippingHouse?.Id,
                            ShippingRegion = shippingHouse?.Region,
                            ShippingCity = shippingHouse?.City,
                            ShippingAddress = shippingHouse?.Address,
                            DeliveryWarehouseId = warehouse.Id,
                            DeliveryAddress = delivery.Consignee.Address,
                            DeliveryCity = delivery.Consignee.City,
                            DeliveryRegion = delivery.Consignee.Region,
                        };
                        var items = delivery.Positions?.Select(x =>
                            new OrderItem
                            {
                                Id = Guid.NewGuid(),
                                Nart = x.Nart,
                                Description = x.Description,
                                OrderId = newOrder.Id
                            }).ToList();
                        orderItems.AddRange(items);
                        result.Add(newOrder);
                    }
                }
            }
            return result;
        }

        private void RegisterInboundFile(string requestData, List<RouteItem> requestDto)
        {
            var userId = _userProvider.GetCurrentUserId();
            var parsedContent = JsonConvert.SerializeObject(requestDto);
            var inboundFile = new InboundFile
            {
                Id = Guid.NewGuid(),
                Type = "ImportShippings",
                RawContent = requestData,
                ParsedContent = parsedContent,
                UserId = userId,
                ReceivedAtUtc = DateTime.UtcNow
            };
            _dataService.GetDbSet<InboundFile>().Add(inboundFile);
        }

        private decimal ToDecimal(string str)
        {
            var result = Convert.ToDecimal(str.Replace(".", ","));
            return result;
        }
    }
}