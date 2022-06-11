using Domain.Extensions;

namespace Domain.Enums
{
    /// <summary>
    /// Статус заказа
    /// </summary>
    public enum OrderState
    {
        /// <summary>
        /// Не подтверждён
        /// </summary>
        [StateColor(AppColor.Grey), OrderNumber(0)]
        Draft = 0,

        /// <summary>
        /// Отменён
        /// </summary>
        [StateColor(AppColor.Brown), OrderNumber(1)]
        Canceled = 1,

        /// <summary>
        /// Создан
        /// </summary>
        [StateColor(AppColor.Blue), OrderNumber(2)]
        Created = 2,

        /// <summary>
        /// В перевозке
        /// </summary>
        [StateColor(AppColor.Purple), OrderNumber(4)]
        InShipping = 3,

        /// <summary>
        /// Отгружен
        /// </summary>
        [StateColor(AppColor.Orange), OrderNumber(5)]
        Shipped = 4,

        /// <summary>
        /// Доставлен
        /// </summary>
        [StateColor(AppColor.Green), OrderNumber(6)]
        Delivered = 5,

        /// <summary>
        /// В архиве
        /// </summary>
        [StateColor(AppColor.Teal), OrderNumber(7)]
        Archive = 6,

        /// <summary>
        /// Полный возврат
        /// </summary>
        [StateColor(AppColor.Olive), OrderNumber(8)]
        FullReturn = 7,

        /// <summary>
        /// Потерян
        /// </summary>
        [StateColor(AppColor.Red), OrderNumber(9)]
        Lost = 8,

        /// <summary>
        /// Подтверждён
        /// </summary>
        [StateColor(AppColor.Green), OrderNumber(3)]
        Confirmed = 9,
    }
}