using Newtonsoft.Json;
using System.Collections.Generic;

namespace Application.Services.Import.ImportObject
{
    public class TransItem
    {
        /// <summary>
        /// Номер маршрута
        /// </summary>
        [JsonProperty("Antor_Number")]
        public string RouteNumber { get; set; }

        /// <summary>
        /// Номер перевозки
        /// </summary>
        [JsonProperty("Sap_Number")]
        public string ShippingNumber { get; set; }

        [JsonProperty("Lifting_Capacity")]
        public string LiftingCapacity { get; set; }

        [JsonProperty("Deliveries")]
        public List<Delivery> Deliveries { get; set; }
    }
}