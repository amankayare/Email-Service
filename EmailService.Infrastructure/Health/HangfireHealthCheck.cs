using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmailService.Infrastructure.Health
{
    public class HangfireHealthCheck : IHealthCheck
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

                var monitoringApi = storage.GetMonitoringApi();
                var queues = monitoringApi.Queues(); // may throw if not reachable

                return Task.FromResult(HealthCheckResult.Healthy("Hangfire storage reachable"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire storage unreachable", ex));
            }
        }
    }
}
