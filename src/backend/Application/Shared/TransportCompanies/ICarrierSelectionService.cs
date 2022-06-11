using Domain.Persistables;
using System;
using System.Collections.Generic;

namespace Application.Shared.TransportCompanies
{
    public interface ICarrierSelectionService
    {
        Guid? FindCarrier(Shipping shipping, List<Order> orders, out Tariff tariff, out CarrierSelectionType type, params Guid?[] ignoredCarrierIds);
        void UpdateCarrier(Shipping shipping, List<Order> orders, Guid carrierId, Tariff tariff = null);
        void FindAndUpdateCarrier(Shipping shipping, List<Order> orders = null, params Guid?[] ignoredCarrierIds);
    }
}