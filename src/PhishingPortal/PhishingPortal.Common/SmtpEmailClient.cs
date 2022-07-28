namespace PhishingPortal.Common
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Net;
    using System.Net.Mail;

    public class SmtpEmailClient : IEmailClient
    {
        public SmtpEmailClient(ILogger<SmtpEmailClient> logger, IConfiguration config)
        {
            Logger = logger;

            _smtpConfig = new SmtpClientConfig(config, logger);
            SmtpClient = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port);

            if (SmtpClient.EnableSsl)
            {
                Logger.LogInformation($"EnableSsl: True");
                SmtpClient.EnableSsl = _smtpConfig.EnableSsl;
            }
               

            if (_smtpConfig.UseDefaultCredentials)
            {
                Logger.LogInformation($"UseDefaultCredentials: True");
                SmtpClient.UseDefaultCredentials = true;
            }
            else
            {
                Logger.LogInformation($"NetworkCredentials Applied");
                SmtpClient.Credentials = new NetworkCredential(_smtpConfig.From, _smtpConfig.Password);
            }
        }

        readonly SmtpClientConfig _smtpConfig;
        public ILogger<SmtpEmailClient> Logger { get; }
        public SmtpClient SmtpClient { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <param name="correlationId"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(string email, string subject, string content, bool isHtml = true, string correlationId = "", string from = "")
        {
            
            Logger.LogInformation($"SendEmailAsync - Start");
           
            if (string.IsNullOrEmpty(from))
                from = _smtpConfig.From;

            var msg = new MailMessage();
           
            Logger.LogInformation($"MailTo: {email}");
            msg.Priority = MailPriority.Normal;
            msg.IsBodyHtml = isHtml;

            if (isHtml)
            {
                var altView = content.ToMultipartMailBody(_smtpConfig.ImageRoot);
                msg.AlternateViews.Add(altView);
            }
            else
            {
                msg.Body = content;
            }
            Logger.LogDebug($"Mail Body Content - {content}");

            msg.Subject = subject;

            Logger.LogDebug($"Mail Subject - {subject}");
            
            msg.To.Add(new MailAddress(email));
            msg.Sender = new MailAddress(from);

            msg.BodyEncoding = System.Text.Encoding.UTF8;

            if(!_smtpConfig.IsSendingEnabled)
               Logger.LogWarning("Email sending is not enabled in configuration, please refer to appsettings.json");
            else 
                await SendEmailAsync(msg);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(MailMessage message, string correlationId = "")
        {
            try
            {
                if (message.From == null)
                    message.From = new MailAddress(_smtpConfig.From);

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