using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhishingPortal.Common;
using PhishingPortal.Dto;
using PhishingPortal.Services.Utilities.Helper;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Utilities.WeeklyReport
{
    internal class WeeklySummaryReports : IWeeklySummaryReports
    {
        private string _sqlLiteDbPath { get; } = "D:/Credent/Git/PhishingPortal/src/PhishingPortal/PhishingPortal.Server/App_Data";
        public ILogger<WeeklySummaryReports> Logger { get; }
        public IEmailClient EmailSender { get; }
        public Tenant Tenant { get; }
        public ITenantDbConnManager ConnManager { get; }

        public ConcurrentQueue<EmailCampaignInfo> Queue { get; }
        readonly Task _processTask;
        readonly int _executorDelayInSeconds = 5;
        bool _stopped;


        public WeeklySummaryReports(ILogger<WeeklySummaryReports> logger,
           IConfiguration config, Tenant tenant, ITenantDbConnManager connManager)
        {
            Logger = logger;
            Tenant = tenant;
            ConnManager = connManager;
            _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");
        }
        public async Task ExecuteTask(CancellationToken stopppingToken)
        {
            await Task.Run(async () =>
            {
                try
                {
                    
                }
                catch (Exception ex)
                {

                }

            });
        }


        public void Start()
        {
            _stopped = false;
            Logger.LogInformation($"Weekly summary report service starting...");
            Queue.Clear();

            _processTask.Start();
            Logger.LogInformation($"Weekly summary report service started");
        }

        public void Stop()
        {
            _stopped = true;
            Logger.LogInformation($"Weekly summary report service stopping ...");
            if (_processTask != null && _processTask.Status == TaskStatus.Running)
            {
                _processTask.Wait();
            }

            Queue.Clear();
            //TenantDbConnMgr.Dispose();
            Logger.LogInformation($"Weekly summary report service stopped");
        }
    }
}
