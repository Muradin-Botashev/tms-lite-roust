using Application.Shared.Addresses.DaData;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using System.IO;
using System.Net;

namespace Application.Shared.Addresses
{
    public class CleanAddressService : ICleanAddressService
    {
        private readonly IConfiguration _configuration;

        public CleanAddressService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CleanAddressDto CleanAddress(string address)
        {
            using (LogContext.PushProperty("Type", "DaData"))
            {
                CleanAddressDto result = null;
                bool isDaDataEnabled = _configuration.GetValue("Dadata:Enabled", true);
                if (isDaDataEnabled)
                {
                    WebRequest request = CreateCleanAnswerRequest(address);

                    WebResponse response = request.GetResponse();
                    string responseData;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseData = reader.ReadToEnd();
                    }

                    Log.Information("Разбор адреса через КЛАДР запрошен для {address}", address);

                    DaDataCleanAddressAnswer[] answer;
                    answer = JsonConvert.DeserializeObject<DaDataCleanAddressAnswer[]>(responseData);

                    if (answer != null && answer.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(answer[0].PostalCode) || !string.IsNullOrEmpty(answer[0].Fias))
                        {
                            result = ConvertCleanAnswerResponse(answer[0]);
                            Log.Information("Адрес {address} разобран как: {PostalCode}, {Region}, {City}, {Street}, {House}, не разобрано: {UnparsedAddressParts}",
                                            address, result.PostalCode, result.Region, result.City, result.Street, result.House, result.UnparsedAddressParts);
                        }
                        else
                        {
                            Log.Warning("В КЛАДР не определился ни почтовый индекс, ни ФИАС для {address}", address);
                        }
                    }
                    else
                    {
                        Log.Warning("В КЛАДР не найдено вариантов для {address}", address);
                    }
                }
                return result;
            }
        }

        private WebRequest CreateCleanAnswerRequest(string address)
        {
            string token = _configuration.GetValue("Dadata:Token", string.Empty);
            string secret = _configuration.GetValue("Dadata:Secret", string.Empty);
            string url = _configuration.GetValue("Dadata:CleanUrl", string.Empty);

            WebRequest request = WebRequest.Create(url);
            request.Headers["Authorization"] = $"Token {token}";
            request.Headers["X-Secret"] = secret;
            request.Headers["Content-Type"] = "application/json";
            request.Headers["Accept"] = "application/json";
            request.Method = "POST";

            string[] data = new[] { address };
            string jsonData = JsonConvert.SerializeObject(data);

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(jsonData);
            }

            return request;
        }

        private CleanAddressDto ConvertCleanAnswerResponse(DaDataCleanAddressAnswer answer)
        {
            string region = null;
            if (!string.IsNullOrEmpty(answer?.Region))
            {
                region = $"{answer?.Region} {answer?.RegionType}";
            }

            string area = null;
            if (!string.IsNullOrEmpty(answer?.Area))
            {
                area = $"{answer?.Area} {answer?.AreaType}";
            }

            string city;
            if (!string.IsNullOrEmpty(answer?.City))
            {
                city = answer?.City;
            }
            else if (!string.IsNullOrEmpty(answer?.Settlement))
            {
                city = answer?.Settlement;
            }
            else
            {
                city = answer?.Region;
            }

            string street = null;
            if (!string.IsNullOrEmpty(answer?.Street))
            {
                street = $"{answer?.StreetType} {answer?.Street}";
            }

            var address = new CleanAddressDto
            {
                Address = answer?.Result,
                PostalCode = answer?.PostalCode,
                Region = region,
                Area = area,
                City = city,
                Street = street,
                House = answer?.House,
                Latitude = answer?.Latitude,
                Longitude = answer?.Longitude,
                GeoQuality = answer?.GeoQuality,
                UnparsedAddressParts = answer?.UnparsedParts
            };
            return address;
        }
    }
}
