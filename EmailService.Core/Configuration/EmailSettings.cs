namespace EmailService.Core.Configuration;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Provider { get; init; } = "sendgrid";
    public string FromEmail { get; init; } = "noreply@example.com";

    // SendGrid
    public string? SendGridApiKey { get; init; }

    // AWS SES uses standard AWS_* environment variables / SDK resolution.
}

