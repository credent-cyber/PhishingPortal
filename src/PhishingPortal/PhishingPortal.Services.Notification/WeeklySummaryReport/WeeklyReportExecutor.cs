using PhishingPortal.Common;
using PhishingPortal.Dto;
using PhishingPortal.Services.Notification.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.WeeklySummaryReport
{
    public class WeeklyReportExecutor : IWeeklyReportExecutor
    {
        public ConcurrentQueue<WeeklyReportInfo> Queue { get; }
        public ILogger<WeeklyReportExecutor> Logger { get; }
        public IEmailClient EmailSender { get; }
        public ITenantDbConnManager TenantDbConnMgr { get; }

        readonly Task _processTask;
        readonly int _executorDelayInSeconds = 5;
       // readonly MailTrackerConfig _mailTrackerConfig;
        bool _stopped;

        public WeeklyReportExecutor(ILogger<WeeklyReportExecutor> logger, IConfiguration config, IEmailClient emailSender,
           ITenantDbConnManager connManager)
        {
            Logger = logger;
            EmailSender = emailSender;
            TenantDbConnMgr = connManager;
            //_mailTrackerConfig = new MailTrackerConfig(config);
            _processTask = ExecuteTask();
            Queue = new ConcurrentQueue<WeeklyReportInfo>();

            _executorDelayInSeconds = config.GetValue<int>("EmailCampaignExectorDelayInSeconds");
            logger.LogInformation($"Weekly ReportExectorDelayInSeconds: {_executorDelayInSeconds}");
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
                        Logger.LogInformation($"Weekly Report executor - total pending campaigns {Queue.Count()}");
                        if (Queue.TryDequeue(out WeeklyReportInfo ecinfo))
                        {
                            if (ecinfo != null)
                            {
                                //Logger.LogInformation($"Sending email for tenantIdentifier:{ecinfo.Tenantdentifier}, EmailSubject: {ecinfo.EmailSubject}");

                                //var db = TenantDbConnMgr.GetContext(ecinfo.Tenantdentifier);

                                ////if (_mailTrackerConfig.EnableEmbedTracker)
                                ////    ecinfo.EmailContent += EmbedTracker(ecinfo);

                                //await EmailSender.SendEmailAsync(ecinfo.EmailRecipients, ecinfo.EmailSubject, ecinfo.EmailContent, true, ecinfo.LogEntry.SecurityStamp, ecinfo.EmailFrom);

                                //Logger.LogInformation($"Email sent");

                                //ecinfo.LogEntry.Status = CampaignLogStatus.Sent.ToString();
                                //ecinfo.LogEntry.SentOn = DateTime.Now;

                                //db.Add(ecinfo.LogEntry);
                                //db.SaveChanges();
                                //Logger.LogInformation($"CampaignLog with id: [{ecinfo.LogEntry.Id}] updated");
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

        public void OnNext(WeeklyReportInfo value)
        {
            Logger.LogInformation($"Weekly Report queuing, queuing");
            Queue.Enqueue(value);
            Logger.LogInformation($"Weekly Report queued");
        }
    }
}
