using Microsoft.Extensions.Configuration;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Services.Notification.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.Email.AppNotifications
{
    public class AppEventNotifier : IAppEventNotifier
    {
        private readonly ILogger<AppEventNotifier> _logger;
        private readonly IConfiguration _config;
        private readonly IEmailClient _emailSender;
        private readonly CentralDbContext _centralDbContext;
        private readonly ITenantDbConnManager _connManager;

        public AppEventNotifier(ILogger<AppEventNotifier> logger, IConfiguration config, IEmailClient emailSender, CentralDbContext centralDbContext,
            ITenantDbConnManager connManager)
        {
            _logger = logger;
            _config = config;
            _emailSender = emailSender;
            _centralDbContext = centralDbContext;
            _connManager = connManager;
        }

        public async Task CheckAndNotifyErrors()
        {
            var logs = _centralDbContext.AppLogs.Where(o => o.IsEmailed == false);
            var recipients = _config.GetValue<string>("AppErrorRecipients") ?? "malay.pandey@credentinfotech.com";
            var sendErrorNotificationEmails = _config.GetValue<bool>("SendAppErrorNotificationEmails");

            if (sendErrorNotificationEmails)
            {
                try
                {
                    var msg = new MailMessage();

                    try
                    {
                        foreach (var r in recipients.Split(";"))
                        {
                            msg.To.Add(r);
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "There was an error adding recipient for the error notifications emails");
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<table><tr><th>Error Message</th><th>Error Details</th></tr>");

                    try
                    {
                        foreach (var log in logs)
                        {
                            sb.Append($"<tr><td>{log.Message}</td><td>{log.ErrorDetail}</td></tr>");
                            log.IsEmailed = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "There is problem compiling application logs");
                    }

                    sb.Append("</table>");
                    msg.IsBodyHtml = true;
                    msg.Body = sb.ToString();
                    msg.Subject = "Phishsim: Application Error Log";
                    await _emailSender.SendEmailAsync(msg);
                    _centralDbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "There is problem sending error notification");
                }

            }
            else
            {
                _logger.LogWarning("Error notification emails are enabled");
            }
        }
    }
}
