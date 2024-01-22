using PhishingPortal.Dto;
using PhishingPortal.Services.Notification.Helper;
using System.Collections.Concurrent;

namespace PhishingPortal.Services.Notification.Sms
{
    public class SmsCampaignExecutor : ISmsCampaignExecutor
    {
        public ConcurrentQueue<SmsCampaignInfo> Queue { get; }
        public ISmsGatewayClient SmsClient { get; }
        public ILogger Logger { get; }
        public ITenantDbConnManager TenantDbConnMgr { get; }
        public IConfiguration Configuration { get; }

        readonly Task _processTask;
        readonly int _executorDelayInSeconds = 5;
        bool _stopped;

        public SmsCampaignExecutor(ILogger<SmsCampaignExecutor> logger, IConfiguration config, ISmsGatewayClient smsClient,
            ITenantDbConnManager connManager)
        {
            Logger = logger;
            Configuration = config;
            SmsClient = smsClient;
            TenantDbConnMgr = connManager;

            _processTask = ExecuteTask();
            Queue = new ConcurrentQueue<SmsCampaignInfo>();

            _executorDelayInSeconds = config.GetValue<int>("SmsCampaignExectorDelayInSeconds");
            logger.LogInformation($"SmsCampaignExectorDelayInSeconds: {_executorDelayInSeconds}");
            if (_executorDelayInSeconds <= 0)
                _executorDelayInSeconds = 1;
        }

        public void OnCompleted()
        {
            Logger.LogInformation($"On completed");
        }

        public void OnError(Exception error)
        {
            Logger.LogError(error, error.Message);
        }

        public void OnNext(SmsCampaignInfo value)
        {
            Logger.LogInformation($"Sms campaign recieved, queueing");
            Queue.Enqueue(value);
            Logger.LogInformation($"Campaign queued");
        }

        public void Start()
        {
            _stopped = false;
            Logger.LogInformation($"Sms campaign executor starting...");
            Queue.Clear();

            _processTask.Start();
            Logger.LogInformation($"Sms campaign executor started");
        }

        public void Stop()
        {
            _stopped = true;
            Logger.LogInformation($"Sms campaign executor stopping ...");
            if (_processTask != null && _processTask.Status == TaskStatus.Running)
            {
                _processTask.Wait();
            }

            Queue.Clear();
            TenantDbConnMgr.Dispose();
            Logger.LogInformation($"Sms campaign executor stoppped");
        }

        private Task ExecuteTask()
        {
            return new Task(async () =>
            {

                try
                {
                    while (!_stopped)
                    {
                        Logger.LogInformation($"Sms campaign executor - total pending campaigns {Queue.Count()}");
                       
                        if (Queue.TryDequeue(out var smsInfo))
                        {
                            if (smsInfo != null)
                            {
                                Logger.LogInformation($"Sending email for tenantIdentifier:{smsInfo.Tenantdentifier},To:{smsInfo.SmsRecipient},SecurityId:{smsInfo.LogEntry.SecurityStamp}");

                                var db = TenantDbConnMgr.GetContext(smsInfo.Tenantdentifier);

                                // TODO: short urls
                                var outcome = await SmsClient.Send(smsInfo.SmsRecipient, smsInfo.From, smsInfo.SmsContent, smsInfo.TemplateId);

                                if (outcome.Item1)
                                {
                                    Logger.LogInformation($"Sms sent");

                                    smsInfo.LogEntry.SentOn = DateTime.Now;
                                    smsInfo.LogEntry.Status = CampaignLogStatus.Sent.ToString();
                                    smsInfo.LogEntry.MessageId = outcome.Item2; //save MessageId to check msg delivery status later
                                    db.Add(smsInfo.LogEntry);
                                    db.SaveChanges();

                                    Logger.LogInformation($"CampaignLog for sms with id: [{smsInfo.LogEntry.Id}] updated");
                                }
                                else
                                {
                                    // replace in the queue
                                    Queue.Enqueue(smsInfo);
                                    Logger.LogWarning($"Sms couldn't be sent, replaced in the queue");
                                    Logger.LogInformation($"Total Pending Sms: {Queue.Count()}");
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
    }
}