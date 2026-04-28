using System.Net;
using Hangfire.Dashboard;
using Microsoft.Extensions.Configuration;

namespace EmailService.Infrastructure.Security
{
    public class ApiKeyDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _apiKey;

        public ApiKeyDashboardAuthorizationFilter(IConfiguration configuration)
        {
            _apiKey = configuration["Dashboard:ApiKey"] ?? string.Empty;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow local requests
            var remoteIp = httpContext.Connection.RemoteIpAddress;
            if (remoteIp == null || IPAddress.IsLoopback(remoteIp))
            {
                return true;
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                return false;
            }

            // 1. Check Header (for programmatic access)
            if (httpContext.Request.Headers.TryGetValue("X-API-Key", out var headerKey))
            {
                if (_apiKey == headerKey) return true;
            }

            // 2. Check Query String (for browser access)
            if (httpContext.Request.Query.TryGetValue("api_key", out var queryKey))
            {
                if (_apiKey == queryKey) return true;
            }

            return false;
        }
    }
}
