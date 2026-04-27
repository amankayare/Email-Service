using Amazon.SimpleEmail;
using EmailService.Core.Configuration;
using EmailService.Core.Interfaces;
using EmailService.Infrastructure.Providers;
using EmailService.Infrastructure.Services;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SendGrid;

namespace EmailService.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureClientServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Redis for Hangfire (client/dashboard side)
            var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseRedisStorage(redisConnectionString));

            // Queue Service (client-side)
            services.AddScoped<IEmailQueueService, HangfireEmailQueueService>();

            // Providers (register all, select at runtime)
            services.AddAWSService<IAmazonSimpleEmailService>();
            services.AddScoped<AwsSesEmailProvider>();

            services.AddSingleton<ISendGridClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<EmailSettings>>().Value;
                return new SendGridClient(settings.SendGridApiKey);
            });
            services.AddScoped<SendGridEmailProvider>();

            services.AddScoped<IEmailProvider>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<EmailSettings>>().Value;
                var provider = (settings.Provider ?? string.Empty).Trim().ToLowerInvariant();

                return provider switch
                {
                    "awsses" => sp.GetRequiredService<AwsSesEmailProvider>(),
                    _ => sp.GetRequiredService<SendGridEmailProvider>(),
                };
            });

            return services;
        }

        public static IServiceCollection AddInfrastructureServerServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Reuse client registrations and add the Hangfire server for workers
            AddInfrastructureClientServices(services, configuration);
            services.AddHangfireServer();
            return services;
        }
    }
}
