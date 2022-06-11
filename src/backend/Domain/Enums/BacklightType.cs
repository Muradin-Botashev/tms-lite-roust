namespace Domain.Enums
{
    public enum BacklightType
    {
        /// <summary>
        /// Перевод перевозок в статус "Заявка отправлена в ТК"
        /// </summary>
        CarrierRequestSentBacklight = 0,

        /// <summary>
        /// Перевод заказа в статус "Подтвержден"
        /// </summary>
        OrderConfirmedBacklight = 1
    }
}
