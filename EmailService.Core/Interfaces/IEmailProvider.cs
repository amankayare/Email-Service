using EmailService.Core.Models;

namespace EmailService.Core.Interfaces
{
    public interface IEmailProvider
    {
        Task SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default);
    }
}
