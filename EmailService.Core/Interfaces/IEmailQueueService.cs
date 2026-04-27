using EmailService.Core.Models;

namespace EmailService.Core.Interfaces
{
    public interface IEmailQueueService
    {
        Task<string> EnqueueAsync(EmailRequest request, CancellationToken cancellationToken = default);
    }
}
