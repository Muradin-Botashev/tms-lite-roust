namespace Domain.Services.Orders
{
    public class OrderSummaryDto
    {
        /// <summary>
        /// Выбрано заказов - общее количество заказов.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Предварительное количество коробок - сумма по полю Предварительное количество коробок
        /// </summary>
        public decimal BoxesCount { get; set; }

        /// <summary>
        /// Предварительное количество паллет - сумма по полю Предварительное количество паллет
        /// </summary>
        public int PalletsCount { get; set; }

        /// <summary>
        /// Плановый вес, г - сумма по полю Плановый вес, г
        /// </summary>
        public decimal WeightKg { get; set; }

        /// <summary>
        /// Базовая стоимость, без НДС
        /// </summary>
        public decimal DeliveryCost { get; set; }

        /// <summary>
        /// Количество часов простоя машин
        /// </summary>
        public decimal TrucksDowntime { get; set; }

        /// <summary>
        /// Сумма за простой, без НДС
        /// </summary>
        public decimal DowntimeAmount { get; set; }

        /// <summary>
        /// Общая стоимость, без НДС
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}
