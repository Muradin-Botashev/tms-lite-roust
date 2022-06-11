using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Extensions
{
    public class RequestLoggingMiddleware
    {
        readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await FormatRequest(context.Request, context.User);
            await _next.Invoke(context);
        }

        private async Task FormatRequest(HttpRequest request, ClaimsPrincipal user)
        {
            request.EnableBuffering();

            string body = null;
            using (var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1 << 10,
                leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            string userName = user?.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);
            if (string.IsNullOrEmpty(userName))
            {
                userName = "Guest";
            }

            Log.Information($"Запрос от {{userName}}: {{Method}} {{Path}} '{body}'", userName, request.Method, request.Path);
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
