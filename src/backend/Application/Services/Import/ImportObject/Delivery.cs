using Newtonsoft.Json;
using System.Collections.Generic;

namespace Application.Services.Import.ImportObject
{
    public class Delivery
    {
        /// <summary>
        /// ТТН
        /// </summary>
        [JsonProperty("Delivery_num")]
        public string InvoiceAmountExcludingVAT { get; set; }

        /// <summary>
        /// Плановая дата отгрузки
        /// </summary>
        [JsonProperty("Pldate")]
        public string ShippingDate { get; set; }

        /// <summary>
        /// Количество бутылок
        /// </summary>
        [JsonProperty("Quantity")]
        public string BottlesCount { get; set; }

        /// <summary>
        /// Вес, кг
        /// </summary>
        [JsonProperty("Weight")]
        public string WeightKg { get; set; }

        /// <summary>
        /// QUANTITY_9L
        /// </summary>
        [JsonProperty("Quantity_9L")]
        public string Volume9l { get; set; }

        /// <summary>
        /// Условия платежа
        /// </summary>
        [JsonProperty("Pay_Condition")]
        public string PaymentCondition { get; set; }

        /// <summary>
        /// Стоимость груза с НДС
        /// </summary>
        [JsonProperty("Amount")]
        public string OrderAmountExcludingVAT { get; set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        [JsonProperty("Comment")]
        public string DeviationsComment { get; set; }

        [JsonProperty("Consignee")]
        public Consignee Consignee { get; set; }

        [JsonProperty("Positions")]
        public List<Position> Positions { get; set; }
    }
}