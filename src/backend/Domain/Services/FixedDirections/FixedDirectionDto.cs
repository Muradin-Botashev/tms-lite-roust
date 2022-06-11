using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.FixedDirections
{
    public class FixedDirectionDto : IDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Транспортная компания
        /// </summary>
        [FieldType(FieldType.Select, source: nameof(TransportCompanies)), OrderNumber(1), IsRequired]
        public LookUpDto CarrierId { get; set; }

        /// <summary>
        /// Склад отгрузки
        /// </summary>
        [FieldType(FieldType.Select, source: nameof(ShippingWarehouses)), OrderNumber(2)]
        public LookUpDto ShippingWarehouseId { get; set; }

        /// <summary>
        /// Склад доставки
        /// </summary>
        [FieldType(FieldType.Select, source: nameof(Warehouses)), OrderNumber(3)]
        public LookUpDto DeliveryWarehouseId { get; set; }

        /// <summary>
        /// Город отгрузки
        /// </summary>
        [FieldType(FieldType.Select, source: nameof(ShippingWarehouseCity), showRawValue: true), OrderNumber(4)]
        public LookUpDto ShippingCity { get; set; }

        /// <summary>
        /// Город доставки
        /// </summary>
        [FieldType(FieldType.Select, source: nameof(WarehouseCity), showRawValue: true), OrderNumber(5)]
        public LookUpDto DeliveryCity { get; set; }

        /// <summary>
        /// Регион отгрузки
        /// </summary>
        [FieldType(FieldType.Select, source: nameof(ShippingWarehouseRegion), showRawValue: true), OrderNumber(6)]
        public LookUpDto ShippingRegion { get; set; }

        /// <summary>
        /// Регион доставки
        /// </summary>
        [FieldType(FieldType.Select, source: nameof(WarehouseRegion), showRawValue: true), OrderNumber(7)]
        public LookUpDto DeliveryRegion { get; set; }

        /// <summary>
        /// Тип ТС
        /// </summary>
        [FieldType(FieldType.MultiSelect, filterType: FieldType.Select, source: nameof(VehicleTypes)), OrderNumber(8)]
        public IEnumerable<LookUpDto> VehicleTypeIds { get; set; }

        /// <summary>
        /// Квота (%)
        /// </summary>
        [FieldType(FieldType.Number), OrderNumber(9), IsRequired]
        public decimal? Quota { get; set; }

        /// <summary>
        /// Активность
        /// </summary>
        [FieldType(FieldType.Boolean, EmptyValue = EmptyValueOptions.NotAllowed), OrderNumber(10), IsRequired]
        public bool? IsActive { get; set; }
    }
}
