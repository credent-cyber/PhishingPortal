using PhishingPortal.Common;
using PhishingPortal.Dto;
using PhishingPortal.Services.Notification.Helper;
using PhishingPortal.Services.Notification.WeeklySummaryReport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.TrainingReminder
{
    public class TrainingRemindExecutor : ITrainingRemindExecutor
    {
        public ConcurrentQueue<TrainingRemindInfo> Queue { get; }
        public ILogger<TrainingRemindExecutor> Logger { get; }
        public IEmailClient EmailSender { get; }
        public ITenantDbConnManager TenantDbConnMgr { get; }

        readonly Task _processTask;
        readonly int _executorDelayInSeconds = 5;
        // readonly MailTrackerConfig _mailTrackerConfig;
        bool _stopped;


        public TrainingRemindExecutor(ILogger<TrainingRemindExecutor> logger, IConfiguration config, IEmailClient emailSender,
           ITenantDbConnManager connManager)
        {
            Logger = logger;
            EmailSender = emailSender;
            TenantDbConnMgr = connManager;
            //_mailTrackerConfig = new MailTrackerConfig(config);
            _processTask = ExecuteTask();
            Queue = new ConcurrentQueue<TrainingRemindInfo>();

            _executorDelayInSeconds = config.GetValue<int>("EmailCampaignExectorDelayInSeconds");
            logger.LogInformation($"Training Remind DelayInSeconds: {_executorDelayInSeconds}");
            if (_executorDelayInSeconds <= 0)
                _executorDelayInSeconds = 1;
        }

        public void Start()
        {
            _stopped = false;
            Logger.LogInformation($"Training Remind executor starting...");
            Queue.Clear();

            _processTask.Start();
            Logger.LogInformation($"Training Remind executor started");
        }

        public void Stop()
        {
            _stopped = true;
            Logger.LogInformation($"Training Remind executor stopping ...");
            if (_processTask != null && _processTask.Status == TaskStatus.Running)
            {
                _processTask.Wait();
            }

            Queue.Clear();
            TenantDbConnMgr.Dispose();
            Logger.LogInformation($"Training Remind executor stoppped");
        }

        private Task ExecuteTask()
        {
            return new Task(async () =>
            {

                try
                {
                    while (!_stopped)
                    {
                        Logger.LogInformation($"Training Remind executor - total pending campaigns {Queue.Count()}");
                        if (Queue.TryDequeue(out TrainingRemindInfo remindInfo))
                        {
                            if (remindInfo != null)
                            {
                                Logger.LogInformation($"Sending email for tenantIdentifier:{remindInfo.TenantIdentifier}, EmailSubject: Training Reminder");

                                var db = TenantDbConnMgr.GetContext(remindInfo.TenantIdentifier);

                                //if (_mailTrackerConfig.EnableEmbedTracker)
                                //    ecinfo.EmailContent += EmbedTracker(ecinfo);

                                foreach (var recipient in remindInfo.EmailDetails.Recipient.Split(';'))
                                {
                                    await EmailSender.SendEmailAsync(recipient, remindInfo.EmailDetails.Subject, remindInfo.EmailDetails.Content, true, "", remindInfo.EmailDetails.From);
                                    Logger.LogInformation($"Trainig Reminder for {remindInfo.TenantIdentifier} sent to {recipient}");
                                }
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

        public void OnNext(TrainingRemindInfo value)
        {
            Logger.LogInformation($"Training Remind queuing, queuing");
            Queue.Enqueue(value);
            Logger.LogInformation($"Training Remind queued");
        }
    }
}
