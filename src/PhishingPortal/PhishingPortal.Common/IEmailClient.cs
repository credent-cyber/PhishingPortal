namespace PhishingPortal.Common
{
    using System.Net.Mail;
    using System.Threading.Tasks;

    public interface IEmailClient
    {
        Task sendEmailAsync(string email, string subject, string content, bool isHtml = true, string correlationId = "", string from = "");
        Task SendEmailAsync(MailMessage message, string correlationId = "");
    }
}