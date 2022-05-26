namespace PhishingPortal.Services.Notification
{
    using System.Net.Mail;
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string content, string correlationId = "");
        Task SendEmailAsync(MailMessage message, string correlationId = "");
    }
}