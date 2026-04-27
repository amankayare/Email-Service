using EmailService.Core.Configuration;
using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmailService.Infrastructure.Providers
{
    public class SendGridEmailProvider : IEmailProvider
    {
        private readonly ISendGridClient _client;
        private readonly ILogger<SendGridEmailProvider> _logger;
        private readonly string _fromEmail;

        public SendGridEmailProvider(ISendGridClient client, IOptions<EmailSettings> emailSettings, ILogger<SendGridEmailProvider> logger)
        {
            _client = client;
            _logger = logger;
            _fromEmail = emailSettings.Value.FromEmail;
        }

        public async Task SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending email via SendGrid to {To}", request.To);

            var from = new EmailAddress(_fromEmail);
            var to = new EmailAddress(request.To);
            var subject = request.Subject;
            var plainTextContent = request.IsHtml ? null : request.Body;
            var htmlContent = request.IsHtml ? request.Body : null;

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await _client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully via SendGrid");
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email via SendGrid. Status: {StatusCode}. Body: {Body}", response.StatusCode, body);
                throw new System.Exception($"SendGrid failed with status {response.StatusCode}");
            }
        }
    }
}
