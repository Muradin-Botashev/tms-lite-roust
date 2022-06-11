using DAL.Services;
using Domain.Persistables;
using System;
using System.Linq;

namespace Application.Shared.BodyTypes
{
    public class DefaultBodyTypeService : IDefaultBodyTypeService
    {
        private readonly ICommonDataService _dataService;

        public DefaultBodyTypeService(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public BodyType GetDefaultBodyType(Order order)
        {
            return GetDefaultBodyType(
                order?.ShippingDate, 
                order?.ShippingWarehouseId, 
                order?.DeliveryWarehouseId,
                order?.ShippingCity, 
                order?.DeliveryCity, 
                order?.ShippingRegion, 
                order?.DeliveryRegion, 
                order?.CompanyId);
        }

        public BodyType GetDefaultBodyType(
            DateTime? shippingDate,
            Guid? shippingWarehouseId, 
            Guid? deliveryWarehouseId,
            string shippingCity, 
            string deliveryCity,
            string shippingRegion, 
            string deliveryRegion,
            Guid? companyId)
        {
            Tariff winterTariff = null;

            if (shippingWarehouseId != null && deliveryWarehouseId != null)
            {
                winterTariff = _dataService.GetDbSet<Tariff>()
                                           .FirstOrDefault(x => x.ShippingWarehouseId == shippingWarehouseId
                                                                && x.DeliveryWarehouseId == deliveryWarehouseId
                                                                && shippingDate >= x.StartWinterPeriod
                                                                && shippingDate <= x.EndWinterPeriod
                                                                && x.CompanyId == companyId);
            }

            if (winterTariff == null)
            {
                winterTariff = _dataService.GetDbSet<Tariff>()
                                           .FirstOrDefault(x => x.ShipmentCity == shippingCity
                                                                && x.DeliveryCity == deliveryCity
                                                                && x.ShippingWarehouseId == null
                                                                && x.DeliveryWarehouseId == null
                                                                && shippingDate >= x.StartWinterPeriod
                                                                && shippingDate <= x.EndWinterPeriod
                                                                && x.CompanyId == companyId);
            }

            if (winterTariff == null)
            {
                winterTariff = _dataService.GetDbSet<Tariff>()
                                           .FirstOrDefault(x => x.ShipmentRegion == shippingRegion
                                                                && x.DeliveryRegion == deliveryRegion
                                                                && string.IsNullOrEmpty(x.ShipmentCity)
                                                                && string.IsNullOrEmpty(x.DeliveryCity)
                                                                && x.ShippingWarehouseId == null
                                                                && x.DeliveryWarehouseId == null
                                                                && shippingDate >= x.StartWinterPeriod
                                                                && shippingDate <= x.EndWinterPeriod
                                                                && x.CompanyId == companyId);
            }

            var bodyTypeName = winterTariff == null ? "Тент" : "Реф";
            return _dataService.GetDbSet<BodyType>().FirstOrDefault(i => i.Name == bodyTypeName && i.CompanyId == null);
        }
    }
}
