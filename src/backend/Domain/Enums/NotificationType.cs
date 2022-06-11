namespace Domain.Enums
{
    public enum NotificationType
    {
        /// <summary>
        /// Отправка заявки в ТК
        /// </summary>
        SendRequestToCarrier = 1,

        /// <summary>
        /// Включение заказов в перевозку
        /// </summary>
        AddOrdersToShipping = 2,

        /// <summary>
        /// Удаление заказов из перевозки
        /// </summary>
        RemoveOrdersFromShipping = 3,

        /// <summary>
        /// Редактирование данных по заказу
        /// </summary>
        UpdateShippingRequestData = 4,

        /// <summary>
        /// Отмена заявки
        /// </summary>
        RejectShippingRequest = 5,

        /// <summary>
        /// Отмена перевозки
        /// </summary>
        CancelShipping = 6,
    }
}
