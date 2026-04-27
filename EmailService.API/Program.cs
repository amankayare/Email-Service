using EmailService.API.Filters;
using EmailService.Core.Configuration;
using EmailService.Infrastructure;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services
    .AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection(EmailSettings.SectionName))
    .Validate(settings =>
    {
        if (string.IsNullOrWhiteSpace(settings.FromEmail))
        {
            return false;
        }

        var provider = (settings.Provider ?? string.Empty).Trim().ToLowerInvariant();
        if (provider is not ("sendgrid" or "awsses"))
        {
            return false;
        }

        if (provider == "sendgrid" && string.IsNullOrWhiteSpace(settings.SendGridApiKey))
        {
            return false;
        }

        return true;
    }, failureMessage: "Invalid EmailSettings configuration.")
    .ValidateOnStart();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<EmailService.Infrastructure.Health.HangfireHealthCheck>("hangfire")
    .AddCheck<EmailService.Infrastructure.Health.RedisHealthCheck>("redis")
    .AddCheck<EmailService.Infrastructure.Health.EmailProviderHealthCheck>("email_provider");

// Register Custom Filters
builder.Services.AddScoped<ApiKeyAuthFilter>();

// Add Infrastructure Services (Hangfire, Redis, Providers) - client side only
builder.Services.AddInfrastructureClientServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Expose Hangfire Dashboard with API key protection (allows local by default)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new EmailService.Infrastructure.Security.ApiKeyDashboardAuthorizationFilter(builder.Configuration) }
});

// Register health checks (includes Hangfire storage probe)
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
