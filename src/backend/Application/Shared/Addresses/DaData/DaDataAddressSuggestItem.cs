using Newtonsoft.Json;

namespace Application.Shared.Addresses.DaData
{
    public class DaDataAddressSuggestItem
    {
        [JsonProperty("value")]
        public string FullName { get; set; }

        [JsonProperty("data")]
        public DaDataAddressData Data { get; set; }
    }
}
