using Newtonsoft.Json;

namespace Application.Shared.Addresses.DaData
{
    public class DaDataCleanAddressAnswer : DaDataAddressData
    {
        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
