using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Application.Shared.Pooling
{
    public class HttpClientService: IHttpClientService
    {
        public async Task<HttpResponseMessage> Get(string url, NameValueCollection queryParams, NameValueCollection headers = null)
        {
            using (HttpClient client = new HttpClient())
            {
                var query = queryParams != null ? queryParams.AllKeys.Select(i => $"{i}={HttpUtility.UrlEncode(queryParams[i])}") : null;

                var queryString = query != null ? string.Join("&", query) : "";

                foreach (var header in headers.AllKeys)
                {
                    client.DefaultRequestHeaders.Add(header, headers[header]);
                }

                Log.Information("Отправка GET запроса к {url} с параметрами: {queryString}", url, queryString);

                return await client.GetAsync($"{url}?{queryString}");
            }
        }

        public async Task<HttpResponseMessage> Get(string url, object data, NameValueCollection headers = null)
        {
            using (HttpClient client = new HttpClient())
            {
                var values = DtoToDictionary(data);

                var properties = values.Select(i => $"{i.Key}={HttpUtility.UrlEncode(i.Value)}");

                string query = string.Join("&", properties);

                foreach (var header in headers.AllKeys)
                {
                    client.DefaultRequestHeaders.Add(header, headers[header]);
                }

                Log.Information("Отправка GET запроса к {url} с параметрами: {query}", url, query);

                return await client.GetAsync($"{url}?{query}");
            }
        }

        private Dictionary<string, string> DtoToDictionary(object data)
        {
            return data.GetType().GetProperties().AsQueryable()
                    .Where(i => i.GetValue(data, null) != null)
                    .ToDictionary(i => i.Name, i => i.GetValue(data, null).ToString());
        }

        public async Task<HttpResponseMessage> Post(string url, object data, NameValueCollection headers = null)
        {
            using (HttpClient client = new HttpClient())
            {
                var contentData = JsonConvert.SerializeObject(data);
                var content = new StringContent(contentData, Encoding.UTF8, "application/json");
               
                foreach (var header in headers.AllKeys)
                {
                    client.DefaultRequestHeaders.Add(header, headers[header]);
                }

                Log.Information("Отправка POST запроса к {url} с даными: {contentData}", url, contentData);

                return await client.PostAsync(url, content);
            }
        }

        public async Task<HttpResponseMessage> Put(string url, object data, NameValueCollection headers = null)
        {
            using (HttpClient client = new HttpClient())
            {
                var contentData = JsonConvert.SerializeObject(data);
                var content = new StringContent(contentData, Encoding.UTF8, "application/json");

                foreach (var header in headers.AllKeys)
                {
                    client.DefaultRequestHeaders.Add(header, headers[header]);
                }

                Log.Information("Отправка PUT запроса к {url} с даными: {contentData}", url, contentData);

                return await client.PutAsync(url, content);
            }
        }

        public async Task<HttpResponseMessage> Delete(string url, NameValueCollection queryParams = null,  NameValueCollection headers = null)
        {
            using (HttpClient client = new HttpClient())
            {
                foreach (var header in headers.AllKeys)
                {
                    client.DefaultRequestHeaders.Add(header, headers[header]);
                }

                var queryArr = queryParams.AllKeys.Select(i => $"{i}={HttpUtility.UrlEncode(queryParams[i])}");
                string query = string.Join("&", queryArr);

                Log.Information("Отправка DELETE запроса к {url} с параметрами: {query}", url, query);

                return await client.DeleteAsync($"{url}?{query}");
            }
        }
    }
}
