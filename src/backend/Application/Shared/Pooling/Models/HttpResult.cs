using System.Net;

namespace Application.Shared.Pooling.Models
{
    /// <summary>
    /// Http result with data object
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    public class HttpResult<TDto> : HttpResult
    {
        public TDto Result { get; set; }
    }

    /// <summary>
    /// Http result
    /// </summary>
    public class HttpResult
    {
        public string Error { get; set; }

        public bool IsError => !string.IsNullOrEmpty(Error) || StatusCode != HttpStatusCode.OK;


        public HttpStatusCode StatusCode { get; set; }
    }
}
