using EmailService.API.Filters;
using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using EmailService.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public class EmailController : ControllerBase
    {
        private readonly IEmailQueueService _emailQueueService;

        public EmailController(IEmailQueueService emailQueueService)
        {
            _emailQueueService = emailQueueService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequestDto request)
        {
            // Model validation is handled by [ApiController]; map DTO to domain model
            var domainRequest = new EmailRequest
            {
                To = request.To,
                Subject = request.Subject,
                Body = request.Body,
                IsHtml = request.IsHtml,
            };

            if (!string.IsNullOrWhiteSpace(request.From))
            {
                domainRequest.From = request.From;
            }

            var cancellationToken = HttpContext?.RequestAborted ?? CancellationToken.None;
            var jobId = await _emailQueueService.EnqueueAsync(domainRequest, cancellationToken);

            return Accepted(new { Message = "Email request queued successfully", JobId = jobId });
        }
    }
}
