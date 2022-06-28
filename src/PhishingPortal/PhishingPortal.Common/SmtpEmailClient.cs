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

            _smtpConfig = new SmtpClientConfig(config);
            SmtpClient = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port);

            SmtpClient.EnableSsl = _smtpConfig.EnableSsl;

            if (_smtpConfig.UseDefaultCredentials)
            {
                SmtpClient.UseDefaultCredentials = true;
            }
            else
            {
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
            if (string.IsNullOrEmpty(from))
                from = _smtpConfig.From;

            var msg = new MailMessage(from, email);

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

            msg.Subject = subject;
            msg.Sender = new MailAddress(_smtpConfig.From);
            msg.BodyEncoding = System.Text.Encoding.UTF8;
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