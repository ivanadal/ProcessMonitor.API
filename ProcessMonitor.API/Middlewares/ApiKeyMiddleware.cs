using System.IO;
using System.Net;

namespace ProcessMonitor.API.Middlewares
{
    /// <summary>
    /// In real production it should be implemented OAuth2 Client Credentials or JWT Bearer tokens
    /// </summary>
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _apiKey = config["ApiKey"]
                ?? throw new InvalidOperationException("ApiKey is missing from configuration");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            // Skip middleware for Swagger endpoints
            if (path.StartsWith("/swagger") || path.StartsWith("/favicon"))
            {
                await _next(context);
                return;
            }
            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var providedKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing API key");
                return;
            }

            if (!string.Equals(providedKey, _apiKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API key");
                return;
            }

            await _next(context);
        }
    }

}
