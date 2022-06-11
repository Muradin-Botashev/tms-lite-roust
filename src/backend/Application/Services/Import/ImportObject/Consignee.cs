using Newtonsoft.Json;

namespace Application.Services.Import.ImportObject
{
    public class Consignee
    {
        /// <summary>
        /// Клиент
        /// </summary>
        [JsonProperty("Name")]
        public string Client { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        [JsonProperty("Address")]
        public string Address { get; set; }

        /// <summary>
        /// Регион
        /// </summary>
        [JsonProperty("Region")]
        public string Region { get; set; }

        /// <summary>
        /// Город
        /// </summary>
        [JsonProperty("City")]
        public string City { get; set; }
    }
}