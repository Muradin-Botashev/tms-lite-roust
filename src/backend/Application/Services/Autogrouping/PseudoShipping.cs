using Domain.Enums;
using Domain.Persistables;
using Domain.Services.Autogrouping;
using System;
using System.Collections.Generic;

namespace Application.Services.Autogrouping
{
    public class PseudoShipping
    {
        public Guid? CompanyId { get; set; }
        public decimal PalletsCount { get; set; }
        public decimal WeightKg { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public ShippingWarehouse ShippingWarehouse { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public Warehouse DeliveryWarehouse { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryRegion { get; set; }
        public string DeliveryCity { get; set; }
        public VehicleType VehicleType { get; set; }
        public decimal? RouteDistance { get; set; }
        public List<IAutogroupingOrder> Orders { get; set; }
        public Dictionary<TarifficationType, CostData> Costs { get; set; }
        public Dictionary<AutogroupingType, List<CostData>> AllCosts { get; set; }
    }
}
