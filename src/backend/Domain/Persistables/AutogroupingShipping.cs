using Domain.Enums;
using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class AutogroupingShipping : IPersistable
    {
        public Guid Id { get; set; }

        public Guid RunId { get; set; }

        public string ShippingNumber { get; set; }

        [ReferenceType(typeof(TransportCompany))]
        public Guid? CarrierId { get; set; }

        [SortKey(nameof(TransportCompany.Title))]
        public TransportCompany Carrier { get; set; }

        [ReferenceType(typeof(VehicleType))]
        public Guid? VehicleTypeId { get; set; }

        [SortKey(nameof(Persistables.VehicleType.Name))]
        public VehicleType VehicleType { get; set; }

        [ReferenceType(typeof(BodyType))]
        public Guid? BodyTypeId { get; set; }

        [SortKey(nameof(Persistables.BodyType.Name))]
        public BodyType BodyType { get; set; }

        public DateTime? ShippingDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public int? OrdersCount { get; set; }

        public int? PalletsCount { get; set; }

        public decimal? WeightKg { get; set; }

        public string Route { get; set; }

        public AutogroupingType? AutogroupingType { get; set; }

        public TarifficationType? TarifficationType { get; set; }

        public decimal? BestCost { get; set; }

        public decimal? FtlDirectCost { get; set; }

        public string FtlDirectCostMessage { get; set; }

        public decimal? FtlRouteCost { get; set; }

        public int? RouteNumber { get; set; }

        public string FtlRouteCostMessage { get; set; }

        public decimal? LtlCost { get; set; }

        public string LtlCostMessage { get; set; }

        public decimal? PoolingCost { get; set; }

        public string PoolingCostMessage { get; set; }

        public decimal? MilkrunCost { get; set; }

        public string MilkrunCostMessage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
