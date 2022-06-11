using Application.Shared.Pooling.Models;
using Domain.Persistables;
using Domain.Services.Pooling.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;

namespace Application.Shared.Pooling
{
    /// <summary>
    /// Сервис для работы с API пулинга
    /// </summary>
    public class PoolingApiService: IPoolingApiService
    {
        private readonly IConfiguration _configuration;

        private readonly IHttpClientService _httpClientService;

        private readonly string _url;

        public PoolingApiService(IConfiguration configuration, IHttpClientService httpClientService)
        {
            _configuration = configuration;
            _httpClientService = httpClientService;
            _url = _configuration.GetValue<string>("Pooling:Url");
        }

        public string Url => _url;

        /// <summary>
        /// Получить список слотов
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public HttpResult<List<SlotDto>> GetSlots(SlotFilterDto dto, Company company)
        {
            var url = $"{_url}/slots";

            var response = _httpClientService.Get(url, dto, GetDefaultHeaders(company)).Result;

            return GetResult<List<SlotDto>>(response);
        }


        /// <summary>
        /// Получить слот
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public HttpResult<SlotDto> GetSlot(string slotId, Company company)
        {
            var url = $"{_url}/slots/{slotId}";

            var response = _httpClientService.Get(url, null, GetDefaultHeaders(company)).Result;

            return GetResult<SlotDto>(response);
        }

        /// <summary>
        /// Зарезервировать слот
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public HttpResult<ReservationRequestDto> BookSlot(ReservationRequestDto dto, Company company)
        {
            var url = $"{_url}/reservations";

            var response = _httpClientService.Post(url, dto, GetDefaultHeaders(company)).Result;

            return GetResult<ReservationRequestDto>(response);
        }

        private NameValueCollection GetDefaultHeaders(Company company)
        {
            return new NameValueCollection
                {
                    { "Authorization", $"Bearer {company?.PoolingToken}" }
                };
        }

        /// <summary>
        /// Зарезервировать слот
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public HttpResult<ReservationRequestDto> UpdateReservation(ReservationRequestDto dto, Company company)
        {
            var url = $"{_url}/reservations/{dto.Id}";

            var response = _httpClientService.Put(url, dto, GetDefaultHeaders(company)).Result;

            return GetResult<ReservationRequestDto>(response);
        }

        private HttpResult<TDto> GetResult<TDto>(HttpResponseMessage response)
        {
            var result  = response.Content.ReadAsStringAsync().Result;

            var httpResult = new HttpResult<TDto>()
            {
                StatusCode = response.StatusCode,
            };

            if (response.IsSuccessStatusCode)
            {
                httpResult.Result = JsonConvert.DeserializeObject<TDto>(result);
            }
            else
            {
                var error = JsonConvert.DeserializeObject<ErrorDto>(result);
                httpResult.Error = error?.Error ?? response.ReasonPhrase;
            }

            return httpResult;
        }

        private HttpResult GetResult(HttpResponseMessage response)
        {
            var result = response.Content.ReadAsStringAsync().Result;

            var httpResult = new HttpResult()
            {
                StatusCode = response.StatusCode,
            };

            if (!response.IsSuccessStatusCode)
            {
                var error = JsonConvert.DeserializeObject<ErrorDto>(result);
                httpResult.Error = error.Error;
            }

            return httpResult;
        }

        /// <summary>
        /// Отменить бронь
        /// </summary>
        /// <param name="id"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public HttpResult CancelSlot(string id, string number, string foreignId, Company company)
        {
            var url = $"{_url}/reservations/{id}/status";

            var parameters = new
            {
                status = "Canceled"
            };

            var response = _httpClientService.Put(url, parameters, GetDefaultHeaders(company)).Result;

            return GetResult(response);
        }
    }
}
