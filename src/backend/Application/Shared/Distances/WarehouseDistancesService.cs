using Application.Shared.Distances.Models;
using Application.Shared.Addresses;
using DAL.Services;
using Domain.Persistables;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Application.Shared.Distances
{
    public class WarehouseDistancesService : IWarehouseDistancesService
    {
        private readonly ICommonDataService _dataService;
        private readonly ICleanAddressService _addressService;
        private readonly string _distanceMatrixUrl;
        private readonly string _googleToken;

        private List<WarehouseDistance> _warehousesCache = null;
        private List<CityDistance> _citiesCache = null;

        public WarehouseDistancesService(IConfiguration configuration, ICommonDataService dataService, ICleanAddressService addressService)
        {
            _dataService = dataService;
            _addressService = addressService;
            _distanceMatrixUrl = configuration.GetValue<string>("Google:DistanceMatrixUrl");
            _googleToken = configuration.GetValue<string>("Google:Token");
        }

        public decimal? FindDistance(IMapPoint shippingWarehouse, string shippingCity,
                                     IMapPoint deliveryWarehouse, string deliveryCity)
        {
            var result = GetDistanceByWarehouses(shippingWarehouse, deliveryWarehouse);
            if (result == null)
            {
                result = GetDistanceByCities(shippingCity, deliveryCity);
            }
            return result;
        }

        private decimal? GetDistanceByWarehouses(IMapPoint shippingWarehouse, IMapPoint deliveryWarehouse)
        {
            if (shippingWarehouse == null || deliveryWarehouse == null)
            {
                return null;
            }

            EnsureWarehousesCache();
            var entry = _warehousesCache.FirstOrDefault(x => (x.ShippingWarehouseId == shippingWarehouse.Id && x.DeliveryWarehouseId == deliveryWarehouse.Id)
                                                            || (x.ShippingWarehouseId == deliveryWarehouse.Id && x.DeliveryWarehouseId == shippingWarehouse.Id));
            if (entry == null)
            {
                if ((shippingWarehouse.Latitude == null || shippingWarehouse.Longitude == null) && shippingWarehouse.GeoQuality == null)
                {
                    var addressData = _addressService.CleanAddress(shippingWarehouse.Address);
                    shippingWarehouse.Latitude = addressData?.Latitude;
                    shippingWarehouse.Longitude = addressData?.Longitude;
                    shippingWarehouse.GeoQuality = addressData?.GeoQuality;
                }

                if ((deliveryWarehouse.Latitude == null || deliveryWarehouse.Longitude == null) && shippingWarehouse.GeoQuality == null)
                {
                    var addressData = _addressService.CleanAddress(deliveryWarehouse.Address);
                    deliveryWarehouse.Latitude = addressData?.Latitude;
                    deliveryWarehouse.Longitude = addressData?.Longitude;
                    deliveryWarehouse.GeoQuality = addressData?.GeoQuality;
                }

                if (shippingWarehouse.Latitude != null && shippingWarehouse.Longitude != null
                    && deliveryWarehouse.Latitude != null && deliveryWarehouse.Longitude != null)
                {
                    string shippingWarehouseName = shippingWarehouse.WarehouseName;
                    string deliveryWarehouseName = deliveryWarehouse.WarehouseName;
                    Log.Information("Запрос расстояния через Google запрошен для пары складов {shippingWarehouseName} - {deliveryWarehouseName}",
                                    shippingWarehouseName, deliveryWarehouseName);

                    var distance = CalculateDistances(shippingWarehouse.Latitude.Value, shippingWarehouse.Longitude.Value,
                                                      deliveryWarehouse.Latitude.Value, deliveryWarehouse.Longitude.Value);

                    entry = new WarehouseDistance
                    {
                        Id = Guid.NewGuid(),
                        ShippingWarehouseId = shippingWarehouse.Id,
                        DeliveryWarehouseId = deliveryWarehouse.Id,
                        Distance = distance
                    };
                    _dataService.GetDbSet<WarehouseDistance>().Add(entry);
                    _warehousesCache.Add(entry);
                }
            }

            return entry?.Distance;
        }

        private decimal? GetDistanceByCities(string shippingCity, string deliveryCity)
        {
            if (string.IsNullOrEmpty(shippingCity) || string.IsNullOrEmpty(deliveryCity))
            {
                return null;
            }

            EnsureCitiesCache();
            var entry = _citiesCache.FirstOrDefault(x => (x.FromCity == shippingCity && x.ToCity == deliveryCity)
                                                        || (x.FromCity == deliveryCity && x.ToCity == shippingCity));

            if (entry == null)
            {
                var fromCityData = _addressService.CleanAddress(shippingCity);
                var toCityData = _addressService.CleanAddress(deliveryCity);

                if (fromCityData?.Latitude != null && fromCityData?.Longitude != null
                    && toCityData?.Latitude != null && toCityData?.Longitude != null)
                {
                    Log.Information("Запрос расстояния через Google запрошен для пары городов {shippingCity} - {deliveryCity}", shippingCity, deliveryCity);

                    var distance = CalculateDistances(fromCityData.Latitude.Value, fromCityData.Longitude.Value,
                                                      toCityData.Latitude.Value, toCityData.Longitude.Value);

                    entry = new CityDistance
                    {
                        Id = Guid.NewGuid(),
                        FromCity = shippingCity,
                        ToCity = deliveryCity,
                        Distance = distance
                    };
                    _dataService.GetDbSet<CityDistance>().Add(entry);
                    _citiesCache.Add(entry);
                }
            }

            return entry?.Distance;
        }

        private decimal? CalculateDistances(decimal shippingLat, decimal shippingLon,
                                            decimal deliveryLat, decimal deliveryLon)
        {
            try
            {
                var origins = Format(shippingLat, shippingLon);
                var destinations = Format(deliveryLat, deliveryLon);
                var methodUrl = $"{_distanceMatrixUrl}/json?mode=driving&units=metric&key={_googleToken}&origins={origins}&destinations={destinations}";
                var request = WebRequest.Create(methodUrl);
                request.Method = "GET";

                WebResponse response = request.GetResponse();
                string responseData;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    responseData = reader.ReadToEnd();
                }

                var answer = JsonConvert.DeserializeObject<DistanceMatrixResponse>(responseData);

                var result = answer?.rows?.FirstOrDefault()?.elements?.FirstOrDefault();
                return result?.distance?.value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка расчета расстояния в Google");
                return null;
            }
        }

        private string Format(decimal lat, decimal lon)
        {
            string latStr = lat.ToString().Replace(',', '.');
            string lonStr = lon.ToString().Replace(',', '.');
            return $"{latStr},{lonStr}";
        }

        private void EnsureWarehousesCache()
        {
            if (_warehousesCache == null)
            {
                _warehousesCache = _dataService.GetDbSet<WarehouseDistance>().ToList();
            }
        }

        private void EnsureCitiesCache()
        {
            if (_citiesCache == null)
            {
                _citiesCache = _dataService.GetDbSet<CityDistance>().ToList();
            }
        }
    }
}
