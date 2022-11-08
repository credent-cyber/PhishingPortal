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
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace PhishingPortal.Services.Notification.RequestMonitor
{
    public class RequestEmailSender : IRequestEmailSender
    {
        public ILogger<RequestEmailSender> Logger { get; }
        public IEmailClient EmailSender { get; }
        public IDbConnManager ConnectionManager { get; }
        string Notifyemail;

        public RequestEmailSender(ILogger<RequestEmailSender> logger, IConfiguration config, IEmailClient emailSender,
           IDbConnManager dbConnManager)
        {
            Logger = logger;
            EmailSender = emailSender;
            ConnectionManager = dbConnManager;
            Notifyemail = config.GetValue<string>("NotificationEmail");
        }

        public async Task ExecuteTask(string email, string cmpny)
        {
            //return new Task(async () =>
            //{

                try
                {
                    if (email != null)
                    {
                        var EmailSubject = $"PhishSims Demo Request";
                        var EmailContent = "Your Demo Request has been Submitted successfully!<br/>We'll get back to you soon...<br/><br/> Thank you<br/>Team PhishSims";
                        var EmailFrom = "info@phishsims.com";
                        var EmailRecipient = email;
                        var db = ConnectionManager.GetContext();
                        var Name = db.FullName.ToString();
                        var ContactNo = db.ContactNumber.ToString();
                        var RequestorMsg = db.Messages.ToString();
                        await EmailSender.SendEmailAsync(EmailRecipient, EmailSubject, EmailContent, true, Guid.NewGuid().ToString(), EmailFrom);

                        Logger.LogInformation($"Requestor with Email: [{email}] Company: [{cmpny}] notified");
                        var Email = Notifyemail;
                        var Content = "New Demo Request for PhishSims recieved <br/> Requestor Details <hr/>" +
                                      $"Name : {Name}<br/>" +
                                      $"Email : {email}<br/> " +
                                      $"Company Name: {cmpny} <br/>" +
                                      $"ContactNumber : {ContactNo} <br/>" +
                                      $"Requestor Message : {RequestorMsg}";
                        await EmailSender.SendEmailAsync(Email, "Demo Request", Content, true, Guid.NewGuid().ToString(), EmailFrom);
                        Logger.LogInformation($"Requestor with Email: [{email}] Company: [{cmpny}] notified to PhishSims mailbox!");
                        ConnectionManager.SetContext(db);
                    }


                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }
            return;
           // });
        }


    }

}
