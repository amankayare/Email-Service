using EmailService.Infrastructure;
using EmailService.Core.Configuration;

var builder = Host.CreateApplicationBuilder(args);

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

// Add Infrastructure Services (Hangfire Server, Redis, Providers)
builder.Services.AddInfrastructureServerServices(builder.Configuration);

var host = builder.Build();
host.Run();
