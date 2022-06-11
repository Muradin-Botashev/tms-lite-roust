using Domain.Persistables;

namespace Application.Shared.Addresses
{
    public class CleanAddressDto : IAddress
    {
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Area { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string UnparsedAddressParts { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? GeoQuality { get; set; }
    }
}
