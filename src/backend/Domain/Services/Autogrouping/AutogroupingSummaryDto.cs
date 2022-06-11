namespace Domain.Services.Autogrouping
{
    public class AutogroupingSummaryDto
    {
        /// <summary>
        /// Количество заказов
        /// </summary>
        public int OrdersCount { get; set; }

        /// <summary>
        /// Количество перевозок
        /// </summary>
        public int ShippingsCount { get; set; }

        /// <summary>
        /// Количество перевозок
        /// </summary>
        public int PalletsCount { get; set; }

        /// <summary>
        /// Общая сумма
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}
