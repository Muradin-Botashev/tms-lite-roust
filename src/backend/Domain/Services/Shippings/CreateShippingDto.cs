using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Shippings
{
    public class CreateShippingDto
    {
        [FieldType(FieldType.Enum, source: nameof(Enums.TarifficationType)), IsRequired]
        public LookUpDto TarifficationType { get; set; }

        [FieldType(FieldType.Select, source: nameof(ShippingWarehouses))]
        public LookUpDto ShippingWarehouseId { get; set; }

        [FieldType(FieldType.BigText), IsRequired]
        public string ShippingAddress { get; set; }

        [FieldType(FieldType.Select, source: nameof(Warehouses))]
        public LookUpDto DeliveryWarehouseId { get; set; }

        [FieldType(FieldType.BigText), IsRequired]
        public string DeliveryAddress { get; set; }

        [FieldType(FieldType.DateTime)]
        public string ShippingDate { get; set; }

        [FieldType(FieldType.DateTime), IsRequired]
        public string DeliveryDate { get; set; }

        [FieldType(FieldType.Integer)]
        public int? PalletsCount { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? TotalWeightKg { get; set; }

        [FieldType(FieldType.Number)]
        public decimal? TotalOrderAmountExcludingVAT { get; set; }

        [FieldType(FieldType.Select, source: nameof(TransportCompanies)), IsRequired]
        public LookUpDto CarrierId { get; set; }

        [FieldType(FieldType.Select, source: nameof(BodyTypes)), IsRequired]
        public LookUpDto BodyTypeId { get; set; }

        [FieldType(FieldType.Enum, source: nameof(Enums.PoolingProductType))]
        public LookUpDto PoolingProductType { get; set; }

        [FieldType(FieldType.Checkbox)]
        public bool? DistributeDataByOrders { get; set; }

        [FieldType(FieldType.Text), IsRequired]
        public string RouteNumber { get; set; }

        [FieldType(FieldType.Number), IsRequired]
        public int? BottlesCount { get; set; }

        [FieldType(FieldType.Number), IsRequired]
        public decimal? Volume9l { get; set; }

        public List<CreateShippingOrderDto> Orders { get; set; }
    }
}