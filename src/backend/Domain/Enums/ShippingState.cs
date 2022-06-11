using Domain.Extensions;

namespace Domain.Enums
{
    /// <summary>
    /// Статус перевозки
    /// </summary>
    public enum ShippingState
    {
        /// <summary>
        /// Создана
        /// </summary>
        [StateColor(AppColor.Blue), OrderNumber(0)]
        ShippingCreated = 0,

        /// <summary>
        /// Отменена
        /// </summary>
        [StateColor(AppColor.Red), OrderNumber(1)]
        ShippingCanceled = 1,

        /// <summary>
        /// Заявка отправлена
        /// </summary>
        [StateColor(AppColor.Purple), OrderNumber(2)]
        ShippingRequestSent = 2,

        /// <summary>
        /// Подтверждена
        /// </summary>
        [StateColor(AppColor.Green), OrderNumber(3)]
        ShippingConfirmed = 3,

        /// <summary>
        /// Отклонена ТК
        /// </summary>
        [StateColor(AppColor.Grey), OrderNumber(4)]
        ShippingRejectedByTc = 4,

        /// <summary>
        /// Завершена
        /// </summary>
        [StateColor(AppColor.Blue), OrderNumber(5)]
        ShippingCompleted = 5,

        /// <summary>
        /// Счёт выставлен
        /// </summary>
        [StateColor(AppColor.Olive), OrderNumber(6)]
        ShippingBillSend = 6,

        /// <summary>
        /// В архиве
        /// </summary>
        [StateColor(AppColor.Teal), OrderNumber(7)]
        ShippingArhive = 7,

        /// <summary>
        /// Срыв поставки
        /// </summary>
        [StateColor(AppColor.Brown), OrderNumber(8)]
        ShippingProblem = 8,

        /// <summary>
        /// Слот забронирован
        /// </summary>
        [StateColor(AppColor.Purple), OrderNumber(9)]
        ShippingSlotBooked = 9,

        /// <summary>
        /// Бронь слота отменена
        /// </summary>
        [StateColor(AppColor.Grey), OrderNumber(10)]
        ShippingSlotCancelled = 10,

        /// <summary>
        /// Согласование изменений
        /// </summary>
        [StateColor(AppColor.Grey), OrderNumber(11)]
        ShippingChangesAgreeing = 11,
    }
}