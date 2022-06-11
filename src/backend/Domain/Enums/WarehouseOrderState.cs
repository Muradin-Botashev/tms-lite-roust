using Domain.Extensions;

namespace Domain.Enums
{
    public enum WarehouseOrderState
    {
        /// <summary>
        /// Не указан
        /// </summary>
        [OrderNumber(0)]
        WhOrderEmpty = 0,

        /// <summary>
        /// PODB-25-ПОДБИРАЕТСЯ
        /// </summary>
        [OrderNumber(1)]
        WhOrderPackaging = 1,

        /// <summary>
        /// PODB-28-ПОД.С.НЕДОБ
        /// </summary>
        [OrderNumber(2)]
        WhOrderPackagingPartial = 2,

        /// <summary>
        /// PODB-30-ОЖ.ОТГРУЗКИ
        /// </summary>
        [OrderNumber(3)]
        WhOrderWaitShipping = 3,

        /// <summary>
        /// PODB-41-ОТГРУЖ.ЧАСТЬ
        /// </summary>
        [OrderNumber(4)]
        WhOrderShippingPartial = 4,

        /// <summary>
        /// PODB-99-ВЫПОЛНЕН
        /// </summary>
        [OrderNumber(5)]
        WhOrderComplete = 5
    }
}
