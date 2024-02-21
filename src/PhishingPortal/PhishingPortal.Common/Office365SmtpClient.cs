using System.Net.Mail;

namespace PhishingPortal.Common
{
    using EASendMail;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class Office365SmtpClient : IEmailClient
    {
        readonly SmtpClientConfig _config;

        public Office365SmtpClient(ILogger<Office365SmtpClient> logger, IConfiguration config)
        {
            _config = new SmtpClientConfig(config, logger);
            Logger = logger;
        }

        public ILogger<Office365SmtpClient> Logger { get; }

        public async Task sendEmailAsync(string to, string subject, string content, bool isHtml = true, string correlationId = "", string from = "")
        {
            Send(to, subject, content, ishtml: isHtml, correlationId: correlationId, from: from);
            await Task.CompletedTask;
        }

        public async Task SendEmailAsync(MailMessage message, string correlationId)
        {
            var to = string.Join(';', message.To.Select(o => o.Address));
            Send(to, message.Subject, message.Body, correlationId: correlationId);
            await Task.CompletedTask;
        }

        private void Send(string to, string subject, string content, bool ishtml = true, string correlationId = "", string from = "")
        {
            try
            {
                SmtpMail oMail = new SmtpMail(_config.LicenseKey);

                oMail.From = string.IsNullOrEmpty(from) ? _config.From : from;

#if DEBUG
                oMail.From = _config.From;
#endif
                oMail.To = to;

                oMail.Subject = subject;

                if (ishtml)
                {
                    var mpc = content.ToMimePartCollection(_config.ImageRoot);

                    oMail.HtmlBody = mpc.Item2;

                    foreach (var mp in mpc.Item1)
                    {
                        oMail.MimeParts.Add(mp);
                    }
                }
                else
                {
                    oMail.TextBody = content;
                }

                SmtpServer oServer = new SmtpServer(_config.Server);

                oServer.Port = _config.Port;

                if (_config.UseDefaultCredentials)
                {
                    oServer.UseDefaultCredentials = true;
                }
                else
                {
                    oServer.User = _config.User;
                    oServer.Password = _config.Password;

                    if (_config.ConnectType != -1
                        && Enum.TryParse<SmtpConnectType>(_config.ConnectType.ToString(), out SmtpConnectType ctype))
                    {
                        oServer.ConnectType = ctype;
                    }

                }

                Logger.LogInformation($"start to send email over SSL... correlationID:[{correlationId}]");

                SmtpClient oSmtp = new SmtpClient();

                if (!_config.IsSendingEnabled)
                    Logger.LogWarning("Email sending is not enabled in configuration, please refer to appsettings.json");
                else
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