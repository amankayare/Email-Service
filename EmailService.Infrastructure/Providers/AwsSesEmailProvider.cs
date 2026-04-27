using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using EmailService.Core.Configuration;
using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmailService.Infrastructure.Providers
{
    public class AwsSesEmailProvider : IEmailProvider
    {
        private readonly IAmazonSimpleEmailService _client;
        private readonly ILogger<AwsSesEmailProvider> _logger;
        private readonly string _fromEmail;

        public AwsSesEmailProvider(IAmazonSimpleEmailService client, IOptions<EmailSettings> emailSettings, ILogger<AwsSesEmailProvider> logger)
        {
            _client = client;
            _logger = logger;
            _fromEmail = emailSettings.Value.FromEmail;
        }

        public async Task SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending email via AWS SES to {To}", request.To);

            var sendRequest = new SendEmailRequest
            {
                Source = _fromEmail,
                Destination = new Destination
                {
                    ToAddresses = new System.Collections.Generic.List<string> { request.To }
                },
                Message = new Message
                {
                    Subject = new Content(request.Subject),
                    Body = new Body
                    {
                        Html = request.IsHtml ? new Content(request.Body) : null,
                        Text = !request.IsHtml ? new Content(request.Body) : null
                    }
                }
            };

            try
            {
                var response = await _client.SendEmailAsync(sendRequest, cancellationToken);
                _logger.LogInformation("Email sent successfully via AWS SES. MessageId: {MessageId}", response.MessageId);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via AWS SES.");
                throw; // Rethrow to allow Hangfire to retry
            }
        }
    }
}
