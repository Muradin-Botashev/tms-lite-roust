using Newtonsoft.Json;
using System.Collections.Generic;

namespace Application.Shared.Addresses.DaData
{
    public class DaDataAddressSuggestAnswer
    {
        [JsonProperty("suggestions")]
        public List<DaDataAddressSuggestItem> Items { get; set; }
    }
}
