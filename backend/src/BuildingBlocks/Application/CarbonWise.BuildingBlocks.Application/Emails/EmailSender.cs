using CarbonWise.BuildingBlocks.Application.Emails;
using Microsoft.Extensions.Logging;

namespace CarbonWise.BuildingBlocks.Infrastructure.Emails
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly EmailsConfiguration _configuration;

        public EmailSender(
            ILogger<EmailSender> logger,
            EmailsConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(EmailMessage message)
        {
            // TODO: Implement actual email sending logic
            _logger.LogInformation($"Email: To: {message.To}, Subject: {message.Subject}, Content: {message.Content}");
            await Task.CompletedTask;
        }
    }
}