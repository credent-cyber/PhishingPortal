using System.Net.Mail;

namespace PhishingPortal.Services.Notification
{

    using EASendMail;
    public class Office365SmtpClient : IEmailSender
    {
        readonly SmtpClientConfig _config;
        public Office365SmtpClient(ILogger<Office365SmtpClient> logger, IConfiguration config)
        {
            _config = new SmtpClientConfig(config);
            Logger = logger;
        }

        public ILogger<Office365SmtpClient> Logger { get; }

        public async Task SendEmailAsync(string to, string subject, string content, string correlationId)
        {
            Send(to, subject, content, correlationId: correlationId);
            await Task.CompletedTask;
        }

        public async Task SendEmailAsync(MailMessage message, string correlationId)
        {
            var to = string.Join(';', message.To.Select(o => o.Address));
            Send(to, message.Subject, message.Body, correlationId: correlationId);
            await Task.CompletedTask;
        }

        private void Send(string to, string subject, string content, bool ishtml = true, string correlationId = "")
        {
            try
            {
                SmtpMail oMail = new SmtpMail(_config.LicenseKey);

                oMail.From = _config.From; 
                oMail.To = to;

                oMail.Subject = subject;

                if (ishtml)
                    oMail.HtmlBody = content;
                else
                    oMail.TextBody = content;

                SmtpServer oServer = new SmtpServer(_config.Server);

                oServer.User = _config.User;

                oServer.Password = _config.Password;

                oServer.Port = _config.Port;

                // detect SSL/TLS connection automatically
                oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

                Logger.LogInformation($"start to send email over SSL... correlationID:[{correlationId}]");

                SmtpClient oSmtp = new SmtpClient();
                oSmtp.SendMail(oServer, oMail);

                Logger.LogInformation($"Email with CorrelationID:[{correlationId}] was sent successfully!");
            }
            catch (Exception ep)
            {
                Logger.LogCritical("failed to send email with the following error:");
                Logger.LogError(ep.Message);
                throw;
            }
        }
    }
}