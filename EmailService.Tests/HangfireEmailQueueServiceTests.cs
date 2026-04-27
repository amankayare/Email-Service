using System.Linq.Expressions;
using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using EmailService.Infrastructure.Services;
using Hangfire;
using Moq;
using Xunit;

namespace EmailService.Tests
{
    public class HangfireEmailQueueServiceTests
    {
        [Fact]
        public async Task EnqueueAsync_ReturnsJobId_FromBackgroundJobClient()
        {
            var bgClientMock = new Mock<IBackgroundJobClient>();
            bgClientMock
                .Setup(c => c.Enqueue<IEmailProvider>(It.IsAny<Expression<Action<IEmailProvider>>>()))
                .Returns("job-123");

            var service = new HangfireEmailQueueService(bgClientMock.Object);
            var req = new EmailRequest { To = "test@example.com", Subject = "s", Body = "b" };

            var jobId = await service.EnqueueAsync(req);

            Assert.Equal("job-123", jobId);
            bgClientMock.Verify(c => c.Enqueue<IEmailProvider>(It.IsAny<Expression<Action<IEmailProvider>>>()), Times.Once);
        }
    }
}
