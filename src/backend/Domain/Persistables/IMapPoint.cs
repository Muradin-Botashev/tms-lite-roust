using System;

namespace Domain.Persistables
{
    public interface IMapPoint
    {
        Guid Id { get; set; }
        string WarehouseName { get; set; }
        string Address { get; set; }
        decimal? Latitude { get; set; }
        decimal? Longitude { get; set; }
        int? GeoQuality { get; set; }
    }
}