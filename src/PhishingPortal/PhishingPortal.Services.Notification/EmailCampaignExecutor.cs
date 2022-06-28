using PhishingPortal.Common;
using System.Collections.Concurrent;

namespace PhishingPortal.Services.Notification
{

    public class EmailCampaignExecutor : IEmailCampaignExecutor
    {
        public ConcurrentQueue<EmailCampaignInfo> Queue { get; }
        public ILogger<EmailCampaignExecutor> Logger { get; }
        public IEmailClient EmailSender { get; }
        public ITenantDbConnManager TenantDbConnMgr { get; }

        readonly Task _processTask;
        readonly int _executorDelayInSeconds = 5;
        bool _stopped;
        public EmailCampaignExecutor(ILogger<EmailCampaignExecutor> logger, IConfiguration config, IEmailClient emailSender,
            ITenantDbConnManager connManager)
        {
            Logger = logger;
            EmailSender = emailSender;
            TenantDbConnMgr = connManager;
            _processTask = ExecuteTask();
            Queue = new ConcurrentQueue<EmailCampaignInfo>();

            _executorDelayInSeconds = config.GetValue<int>("EmailCampaignExectorDelayInSeconds");
            if (_executorDelayInSeconds <= 0)
                _executorDelayInSeconds = 1;
        }

        public void Start()
        {
            _stopped = false;
            Logger.LogInformation($"Email campaign executor starting...");
            Queue.Clear();

            _processTask.Start();
            Logger.LogInformation($"Email campaign executor started");
        }

        public void Stop()
        {
            _stopped = true;
            Logger.LogInformation($"Email campaign executor stopping ...");
            if (_processTask != null && _processTask.Status == TaskStatus.Running)
            {
                _processTask.Wait();
            }

            Queue.Clear();
            TenantDbConnMgr.Dispose();
            Logger.LogInformation($"Email campaign executor stoppped");
        }

        private Task ExecuteTask()
        {
            return new Task(() =>
            {

                try
                {
                    while (!_stopped)
                    {
                        Logger.LogInformation($"Email campaign executor - total pending campaigns {Queue.Count()}");
                        if (Queue.TryDequeue(out EmailCampaignInfo ecinfo))
                        {
                            if (ecinfo != null)
                            {
                                var db = TenantDbConnMgr.GetContext(ecinfo.Tenantdentifier);
                                EmailSender.SendEmailAsync(ecinfo.EmailRecipients, ecinfo.EmailSubject, ecinfo.EmailContent, true, ecinfo.LogEntry.ReturnUrl, ecinfo.EmailFrom);
                                ecinfo.LogEntry.SentOn = DateTime.Now;
                                db.Add(ecinfo.LogEntry);
                                db.SaveChanges();
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
            Logger.LogInformation($"Email campaign recieved, queuing");
            Queue.Enqueue(value);
            Logger.LogInformation($"Campaign queued");
        }
    }
}
