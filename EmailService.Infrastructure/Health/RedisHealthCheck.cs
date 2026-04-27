using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmailService.Infrastructure.Health
{
    public class RedisHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var storage = Hangfire.JobStorage.Current;
                if (storage == null)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire storage is not configured"));
                }

                // Attempt to call a monitoring API method which will fail if Redis is unreachable
                var monitoringApi = storage.GetMonitoringApi();
                var queues = monitoringApi.Queues();

                return Task.FromResult(HealthCheckResult.Healthy("Redis (Hangfire storage) reachable"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Redis (Hangfire storage) unreachable", ex));
            }
        }
    }
}
