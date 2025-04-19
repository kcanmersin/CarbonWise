namespace CarbonWise.BuildingBlocks.Application.Emails
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailMessage message);
    }
}