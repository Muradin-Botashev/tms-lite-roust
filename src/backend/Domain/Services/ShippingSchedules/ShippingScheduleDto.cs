using Domain.Enums;
using Domain.Extensions;
using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.ShippingSchedules
{
    public class ShippingScheduleDto : IDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Город отгрузки
        /// </summary>
        [FieldType(FieldType.Select, source: nameof(ShippingWarehouseCity), showRawValue: true), OrderNumber(1), IsRequired]
        public LookUpDto ShippingCity { get; set; }

        /// <summary>
        /// Город доставки
        /// </summary>
        [FieldType(FieldType.Select, source: nameof(WarehouseCity), showRawValue: true), OrderNumber(2), IsRequired]
        public LookUpDto DeliveryCity { get; set; }

        /// <summary>
        /// Транспортная компания
        /// </summary>
        [FieldType(FieldType.Select, source: nameof(TransportCompanies)), OrderNumber(3), IsRequired]
        public LookUpDto CarrierId { get; set; }

        /// <summary>
        /// День отгрузки
        /// </summary>
        [FieldType(FieldType.MultiEnum, source: nameof(WeekDay)), OrderNumber(4), IsRequired]
        public IEnumerable<LookUpDto> ShippingDays { get; set; }

        /// <summary>
        /// День доставки
        /// </summary>
        [FieldType(FieldType.MultiEnum, source: nameof(WeekDay)), OrderNumber(5), IsRequired]
        public IEnumerable<LookUpDto> DeliveryDays { get; set; }
    }
}
