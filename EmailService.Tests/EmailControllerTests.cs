using System.Threading.Tasks;
using EmailService.API.Controllers;
using EmailService.API.Models;
using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EmailService.Tests
{
    public class EmailControllerTests
    {
        [Fact]
        public async Task SendEmail_ReturnsAccepted_WithJobId()
        {
            var queueMock = new Mock<IEmailQueueService>();
            queueMock.Setup(q => q.EnqueueAsync(It.IsAny<EmailRequest>(), default)).ReturnsAsync("job-1");

            var controller = new EmailController(queueMock.Object);

            var dto = new EmailRequestDto { To = "a@b.com", Subject = "hi", Body = "body" };

            var result = await controller.SendEmail(dto);

            var accepted = Assert.IsType<AcceptedResult>(result);
            Assert.NotNull(accepted.Value);
        }
    }
}
