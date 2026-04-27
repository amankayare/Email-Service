using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmailService.Infrastructure.Health
{
    public class EmailProviderHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public EmailProviderHealthCheck(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var providerName = _configuration["EmailSettings:Provider"]?.ToLowerInvariant() ?? "sendgrid";

                // Basic readiness checks depending on configured provider
                if (providerName == "awsses")
                {
                    // Ensure AWS SES client is registered and basic config exists
                    var ses = _serviceProvider.GetService(typeof(Amazon.SimpleEmail.IAmazonSimpleEmailService));
                    if (ses == null)
                    {
                        return Task.FromResult(HealthCheckResult.Unhealthy("AWS SES client not available"));
                    }
                    return Task.FromResult(HealthCheckResult.Healthy("AWS SES client available"));
                }

                // Default: SendGrid
                var apiKey = _configuration["EmailSettings:SendGridApiKey"];
                var sgClient = _serviceProvider.GetService(typeof(SendGrid.ISendGridClient));

                if (string.IsNullOrEmpty(apiKey) || sgClient == null)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy("SendGrid client or API key not configured"));
                }

                return Task.FromResult(HealthCheckResult.Healthy("Email provider configured"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Email provider readiness check failed", ex));
            }
        }
    }
}
