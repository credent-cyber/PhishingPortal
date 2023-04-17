using PhishingPortal.Common;
using PhishingPortal.Dto;
using PhishingPortal.Services.Notification.Helper;
using PhishingPortal.Services.Notification.Monitoring;
using System.Collections.Concurrent;

namespace PhishingPortal.Services.Notification.Trainings
{
    public class TrainingExecutor : ITrainingExecutor
    {
        public ConcurrentQueue<TraininigInfo> Queue { get; }
        public ILogger<TrainingExecutor> Logger { get; }
        public IEmailClient EmailSender { get; }
        public ITenantDbConnManager TenantDbConnMgr { get; }

        readonly Task _processTask;
        readonly int _executorDelayInSeconds = 5;
        readonly MailTrackerConfig _mailTrackerConfig;
        bool _stopped;

        public TrainingExecutor(ILogger<TrainingExecutor> logger, IConfiguration config, IEmailClient emailSender,
            ITenantDbConnManager connManager)
        {
            Logger = logger;
            EmailSender = emailSender;
            TenantDbConnMgr = connManager;
            _mailTrackerConfig = new MailTrackerConfig(config);
            _processTask = ExecuteTask();
            Queue = new ConcurrentQueue<TraininigInfo>();

            _executorDelayInSeconds = config.GetValue<int>("TrainingExecutorDelayInSeconds");
            logger.LogInformation($"TrainingExecutorDelayInSeconds: {_executorDelayInSeconds}");
            if (_executorDelayInSeconds <= 0)
                _executorDelayInSeconds = 1;
        }

        public void Start()
        {
            _stopped = false;
            Logger.LogInformation($"Training executor starting...");
            Queue.Clear();

            _processTask.Start();
            Logger.LogInformation($"Training executor started");
        }

        public void Stop()
        {
            _stopped = true;
            Logger.LogInformation($"Training executor stopping ...");
            if (_processTask != null && _processTask.Status == TaskStatus.Running)
            {
                _processTask.Wait();
            }

            Queue.Clear();
            TenantDbConnMgr.Dispose();
            Logger.LogInformation($"Training executor stoppped");
        }

        private Task ExecuteTask()
        {
            return new Task(async () =>
            {

                try
                {
                    while (!_stopped)
                    {
                        Logger.LogInformation($"Training executor - total pending Training {Queue.Count()}");
                        if (Queue.TryDequeue(out TraininigInfo trainingInfo))
                        {
                            if (trainingInfo != null)
                            {
                                Logger.LogInformation($"Sending email for tenantIdentifier:{trainingInfo.Tenantdentifier}, EmailSubject: {trainingInfo.TrainingSubject}");

                                var db = TenantDbConnMgr.GetContext(trainingInfo.Tenantdentifier);

                                if (_mailTrackerConfig.EnableEmbedTracker)
                                    trainingInfo.TrainingContent += EmbedTracker(trainingInfo);

                                await EmailSender.SendEmailAsync(trainingInfo.TrainingRecipients, trainingInfo.TrainingSubject, trainingInfo.TrainingContent, true, trainingInfo.TrainingLogEntry.SecurityStamp, trainingInfo.TrainingFrom);

                                Logger.LogInformation($"Training sent");

                                trainingInfo.TrainingLogEntry.SentOn = DateTime.Now;
                                trainingInfo.TrainingLogEntry.Status = TrainingLogStatus.Sent.ToString();

                                db.Add(trainingInfo.TrainingLogEntry);
                                db.SaveChanges();

                                Logger.LogInformation($"Traininglog with id: [{trainingInfo.TrainingLogEntry.Id}] updated");
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

        private string EmbedTracker(TraininigInfo ecinfo)
        {
            return $"<div style='display:none'>{_mailTrackerConfig.MailTrackerBlock}{ecinfo.TrainingLogEntry.SecurityStamp}{_mailTrackerConfig.MailTrackerBlock}</div>";
        }

        public void OnCompleted()
        {
            Logger.LogInformation($"On completed");
        }

        public void OnError(Exception error)
        {
            Logger.LogError(error, error.Message);
        }

        public void OnNext(TraininigInfo value)
        {
            Logger.LogInformation($"Training recieved, queuing");
            Queue.Enqueue(value);
            Logger.LogInformation($"Training queued");
        }




    }
}
