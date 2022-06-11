using DAL.Services;
using Domain.Persistables;
using System;

namespace Application.Shared.Orders
{
    public class OrderFieldsSyncService : IOrderFieldsSyncService
    {
        private readonly ICommonDataService _dataService;

        public OrderFieldsSyncService(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public void SyncWithDeliveryWarehouse(Order order, Warehouse warehouse)
        {
            if (warehouse != null)
            {
                order.ClientName = warehouse.WarehouseName;
                order.PickingFeatures = warehouse.PickingFeatures;
                order.SoldTo = warehouse.SoldToNumber;

                if (warehouse.PickingTypeId.HasValue)
                    order.PickingTypeId = warehouse.PickingTypeId;

                order.TransitDays = warehouse.LeadtimeDays;

                order.DeliveryWarehouseId = warehouse.Id;
                order.DeliveryAddress = warehouse.Address;
                order.DeliveryCity = warehouse.City;
                order.DeliveryRegion = warehouse.Region;
                order.DeliveryType = warehouse.DeliveryType;
            }
            else
            {
                order.DeliveryWarehouseId = null;
            }

            order.OrderChangeDate = DateTime.UtcNow;
        }
    }
}
