using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using Hangfire;

namespace EmailService.Infrastructure.Services
{
    public class HangfireEmailQueueService : IEmailQueueService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireEmailQueueService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public Task<string> EnqueueAsync(EmailRequest request, CancellationToken cancellationToken = default)
        {
            var jobId = _backgroundJobClient.Enqueue<IEmailProvider>(provider => provider.SendEmailAsync(request, default));
            return Task.FromResult(jobId);
        }
    }
}
