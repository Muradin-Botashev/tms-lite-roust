using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Pooling
{
    public interface IHttpClientService
    {
        Task<HttpResponseMessage> Get(string url, NameValueCollection queryParams, NameValueCollection headers = null);

        Task<HttpResponseMessage> Get(string url, object data, NameValueCollection headers = null);

        //Task<HttpResponseMessage> Post(string url, HttpContent content, NameValueCollection headers = null);

        Task<HttpResponseMessage> Post(string url, object data, NameValueCollection headers = null);

        Task<HttpResponseMessage> Put(string url, object data, NameValueCollection headers = null);

        Task<HttpResponseMessage> Delete(string url, NameValueCollection queryParams = null, NameValueCollection headers = null);
    }
}
