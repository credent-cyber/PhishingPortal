using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhishingPortal.Common;
using PhishingPortal.Dto;
using PhishingPortal.Services.Utilities.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Utilities.WeeklyReport
{
    public class WeeklyReportExecutor : IWeeklyReportExecutor
    {
        public ConcurrentQueue<EmailCampaignInfo> Queue { get; }
        public ILogger<WeeklyReportExecutor> Logger { get; }
        public IEmailClient EmailSender { get; }
        public ITenantDbConnManager TenantDbConnMgr { get; }

        readonly Task _processTask;
        readonly int _executorDelayInSeconds = 5;
        //readonly MailTrackerConfig _mailTrackerConfig;
        bool _stopped;

        public WeeklyReportExecutor(ILogger<WeeklyReportExecutor> logger, IConfiguration config, IEmailClient emailSender,
           ITenantDbConnManager connManager)
        {
            Logger = logger;
            EmailSender = emailSender;
            TenantDbConnMgr = connManager;
            //_mailTrackerConfig = new MailTrackerConfig(config);
            _processTask = ExecuteTask();
            Queue = new ConcurrentQueue<EmailCampaignInfo>();

            _executorDelayInSeconds = config.GetValue<int>("WeeklyReportExecutorDelayInSeconds");
            logger.LogInformation($"WeeklyReportExecutorDelayInSeconds: {_executorDelayInSeconds}");
            if (_executorDelayInSeconds <= 0)
                _executorDelayInSeconds = 1;
        }

        public void Start()
        {
            _stopped = false;
            Logger.LogInformation($"Weekly Report executor starting...");
            Queue.Clear();

            _processTask.Start();
            Logger.LogInformation($"Weekly Report executor started");
        }

        public void Stop()
        {
            _stopped = true;
            Logger.LogInformation($"Weekly Report executor stopping ...");
            if (_processTask != null && _processTask.Status == TaskStatus.Running)
            {
                _processTask.Wait();
            }

            Queue.Clear();
            TenantDbConnMgr.Dispose();
            Logger.LogInformation($"Weekly Report executor stoppped");
        }

        private Task ExecuteTask()
        {
            return new Task(async () =>
            {

                try
                {
                    while (!_stopped)
                    {
                        Logger.LogInformation($"Weekly Report executor - total pending {Queue.Count()}");
                        if (Queue.TryDequeue(out EmailCampaignInfo ecinfo))
                        {
                            if (ecinfo != null)
                            {
                                Logger.LogInformation($"Sending Weekly Report for tenantIdentifier:{ecinfo.Tenantdentifier}, EmailSubject: {ecinfo.EmailSubject}");

                                var db = TenantDbConnMgr.GetContext(ecinfo.Tenantdentifier);

                                //if (_mailTrackerConfig.EnableEmbedTracker)
                                //    ecinfo.EmailContent += EmbedTracker(ecinfo);

                                await EmailSender.SendEmailAsync(ecinfo.EmailRecipients, ecinfo.EmailSubject, ecinfo.EmailContent, true, ecinfo.LogEntry.SecurityStamp, ecinfo.EmailFrom);

                                Logger.LogInformation($"Report sent");

                                
                            }

                        }

                        Thread.Sleep(1000 * _executorDelayInSeconds);

                    }

                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                }

            });
        }

        public void OnCompleted()
        {
            Logger.LogInformation($"On completed");
        }

        public void OnError(Exception error)
        {
            Logger.LogError(error, error.Message);
        }

        public void OnNext(EmailCampaignInfo value)
        {
            Logger.LogInformation($"Weekly Report recieved, queuing");
            Queue.Enqueue(value);
            Logger.LogInformation($"Weekly Report queued");
        }
    }
}
