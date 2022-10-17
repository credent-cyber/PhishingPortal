using Microsoft.Extensions.Logging;
using PhishingPortal.Common;
using PhishingPortal.Services.Notification.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Net;
using System.Net.Mail;
using Castle.Core.Smtp;
namespace PhishingPortal.Services.Notification.RequestMonitor
{
    public class RequestEmailSender: IRequestEmailSender
    {
        public ILogger<RequestEmailSender> Logger { get; }
        public IEmailClient EmailSender { get; }
        public IDbConnManager ConnectionManager { get; }
        bool _stopped;
        public RequestEmailSender(ILogger<RequestEmailSender> logger, IConfiguration config, IEmailClient emailSender,
           IDbConnManager dbConnManager)
        {
            Logger = logger;
            EmailSender = emailSender;
            ConnectionManager = dbConnManager;

        }

        public Task ExecuteTask(string email, string cmpny)
        {
            return new Task(async () =>
            {

                try
                {
                    if (email != null)
                    {
                                var EmailSubject = $"PhishSims Demo Request";  
                                var EmailContent = "Your Demo Request has been Submitted successfully!<br/>We'll get back to you soon...<br/><br/> Thank you<br/>Team PhishSims";  
                                var EmailFrom = "info@phishsims.com";  
                                var EmailRecipient = email;  
                                var db = ConnectionManager.GetContext();
                                await EmailSender.SendEmailAsync(EmailRecipient, EmailSubject, EmailContent, true, Guid.NewGuid().ToString(), EmailFrom);

                                Logger.LogInformation($"Requestor with Email: [{email}] Company: [{cmpny}] notified");
                                ConnectionManager.SetContext(db);
                      
                    }
                    

                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }

            });
        }


    }

}
