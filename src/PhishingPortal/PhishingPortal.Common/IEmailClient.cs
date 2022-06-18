namespace PhishingPortal.Common
{
    using System.Net.Mail;
    using System.Threading.Tasks;

    public interface IEmailClient
    {
        Task SendEmailAsync(string email, string subject, string content, string correlationId = "");
        Task SendEmailAsync(MailMessage message, string correlationId = "");
    }
}