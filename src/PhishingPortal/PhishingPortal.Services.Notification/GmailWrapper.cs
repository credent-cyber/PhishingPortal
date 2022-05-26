namespace PhishingPortal.Services.Notification
{
    using System.Net;
    using System.Net.Mail;

    public class GmailWrapper : IEmailSender
    {

        public GmailWrapper(ILogger<GmailWrapper> logger, IConfiguration config)
        {
            Logger = logger;
           
            _smtpConfig = new SmtpClientConfig(config);
            SmtpClient = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port);
            SmtpClient.UseDefaultCredentials = false;
            SmtpClient.EnableSsl = true;
            SmtpClient.Credentials = new NetworkCredential(_smtpConfig.From, _smtpConfig.Password);
           
        }

        readonly SmtpClientConfig _smtpConfig;
        public ILogger<GmailWrapper> Logger { get; }
        public SmtpClient SmtpClient { get; }

        public async Task SendEmailAsync(string email, string subject, string content, string correlationId = "")
        {
            var msg = new MailMessage(_smtpConfig.From, email);

            msg.Priority = MailPriority.Normal;
            msg.Body = content;
            msg.IsBodyHtml = true;
            msg.Subject = subject;
            msg.Sender = new MailAddress(_smtpConfig.From);
            msg.BodyEncoding = System.Text.Encoding.UTF8;

            await SendEmailAsync(msg);
            await Task.CompletedTask;

        }

        public async Task SendEmailAsync(MailMessage message, string correlationId = "")
        {
            try
            {
                Logger.LogInformation($"Mail with correlation id : {correlationId} being send");
                await SmtpClient.SendMailAsync(message);
                Logger.LogInformation($"Mail with correlation id: {correlationId} is send successfully");

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
            await Task.CompletedTask;
        }
    }
}