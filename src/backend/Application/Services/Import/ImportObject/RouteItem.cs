using Newtonsoft.Json;
using System.Collections.Generic;

namespace Application.Services.Import.ImportObject
{
    public class RouteItem
    {
        /// <summary>
        /// Транспортная зона
        /// </summary>
        [JsonProperty("Route")]
        public string TransportZone { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        [JsonProperty("Werks")]
        public string Code { get; set; }

        [JsonProperty("Trans_Items")]
        public List<TransItem> TransItems { get; set; }
    }
}