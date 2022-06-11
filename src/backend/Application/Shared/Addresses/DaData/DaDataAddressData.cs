using Newtonsoft.Json;

namespace Application.Shared.Addresses.DaData
{
    public class DaDataAddressData
    {
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("region_type")]
        public string RegionType { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }

        [JsonProperty("area_type")]
        public string AreaType { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("city_type")]
        public string CityType { get; set; }

        [JsonProperty("settlement")]
        public string Settlement { get; set; }

        [JsonProperty("settlement_type")]
        public string SettlementType { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("street_type")]
        public string StreetType { get; set; }

        [JsonProperty("house")]
        public string House { get; set; }

        [JsonProperty("block")]
        public string Building { get; set; }

        [JsonProperty("flat")]
        public string Office { get; set; }

        [JsonProperty("fias_id")]
        public string Fias { get; set; }

        [JsonProperty("unparsed_parts")]
        public string UnparsedParts { get; set; }

        [JsonProperty("geo_lat")]
        public decimal? Latitude { get; set; }

        [JsonProperty("geo_lon")]
        public decimal? Longitude { get; set; }

        [JsonProperty("qc_geo")]
        public int? GeoQuality { get; set; }
    }
}
