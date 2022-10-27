using PhishingPortal.Core;
using Microsoft.Graph;
using Microsoft.Exchange.WebServices.Data;
using PhishingPortal.Dto;
using PhishingPortal.DataContext;
using Azure.Identity;

namespace PhishingPortal.Services.Notification.RequestMonitor
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using PhishingPortal.Common;
    using PhishingPortal.Services.Notification.Helper;
    using System.Net;
    using System.Net.Mail;
    public class DemoRequestHandler : IDemoRequestHandler
    {
        public ILogger<DemoRequestHandler> Logger { get; }
        private readonly IConfiguration _config;
        private readonly DemoRequestor _demoRequestor;
        private readonly PhishingPortalDbContext phishingPortalDbContext;
        public IRequestEmailSender requestEmailSender;
        public IDbConnManager ConnectionManager { get; set; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DemoRequestHandler(ILogger<DemoRequestHandler> logger, IConfiguration config, IDbConnManager dbConnManager, IRequestEmailSender sender)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Logger = logger;
            _config = config;
            ConnectionManager = dbConnManager;
            requestEmailSender = sender;
        }

        public void Start()
        {
            Logger.LogInformation($"DemoRequestHandler has started");
        }

        public void Stop()
        {
            Logger.LogInformation($"DemoRequestHandler Stopped");
        }

        public void Execute()
        {
            DemoRequestor demo;
            var data = ConnectionManager.GetContext();
            if(data is null)
                return;
            demo = data;
            if (!data.Email.Equals(string.Empty))
            {
                Logger.LogInformation($"New Demo requestor found, Preparing to sent confirmation Email...");
                var name = data.FullName;
                var ID = data.Id;
                var responce = requestEmailSender.ExecuteTask(data.Email, data.Company);
                Logger.LogInformation($"Mail sent!!!!!");
                Logger.LogInformation($"Requestor with Email: [{data.Email}] Company: [{data.Company}] notified");
            }
            var status = ConnectionManager.SetContext(demo);
        }
    }
}
