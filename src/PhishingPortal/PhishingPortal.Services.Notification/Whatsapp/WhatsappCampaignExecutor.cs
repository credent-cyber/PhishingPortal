using PhishingPortal.Dto;
using PhishingPortal.Services.Notification.Helper;
using System.Collections.Concurrent;

namespace PhishingPortal.Services.Notification.Whatsapp
{
    public class WhatsappCampaignExecutor : IWhatsappCampaignExecutor
    {
        public ConcurrentQueue<WhatsappCampaignInfo> Queue { get; }
        public IWhatsappGatewayClient WhatsAppClient { get; }
        public ILogger Logger { get; }
        public ITenantDbConnManager TenantDbConnMgr { get; }
        public IConfiguration Configuration { get; }

        readonly Task _processTask;
        readonly int _executorDelayInSeconds = 5;
        bool _stopped;

        public WhatsappCampaignExecutor(ILogger<WhatsappCampaignExecutor> logger, IConfiguration config, 
            IWhatsappGatewayClient waClient,
            ITenantDbConnManager connManager)
        {
            Logger = logger;
            Configuration = config;
            WhatsAppClient = waClient;
            TenantDbConnMgr = connManager;

            _processTask = ExecuteTask();
            Queue = new ConcurrentQueue<WhatsappCampaignInfo>();

            _executorDelayInSeconds = config.GetValue<int>("WhatsappCampaignExectorDelayInSeconds");
            logger.LogInformation($"WhatsappCampaignExectorDelayInSeconds: {_executorDelayInSeconds}");
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

        public void OnNext(WhatsappCampaignInfo value)
        {
            Logger.LogInformation($"Whatsapp campaign recieved, queueing");
            Queue.Enqueue(value);
            Logger.LogInformation($"Whatsapp Campaign queued");
        }

        public void Start()
        {
            _stopped = false;
            Logger.LogInformation($"Whatsapp campaign executor starting...");
            Queue.Clear();

            _processTask.Start();
            Logger.LogInformation($"Whatsapp campaign executor started");
        }

        public void Stop()
        {
            _stopped = true;
            Logger.LogInformation($"Whatsapp campaign executor stopping ...");
            if (_processTask != null && _processTask.Status == TaskStatus.Running)
            {
                _processTask.Wait();
            }

            Queue.Clear();
            TenantDbConnMgr.Dispose();
            Logger.LogInformation($"Whatsapp campaign executor stoppped");
        }

        private Task ExecuteTask()
        {
            return new Task(async () =>
            {

                try
                {
                    while (!_stopped)
                    {
                        Logger.LogInformation($"Whatsapp campaign executor - total pending campaigns {Queue.Count()}");

                        if (Queue.TryDequeue(out var msgInfo))
                        {
                            if (msgInfo != null)
                            {
                                Logger.LogInformation($"Sending email for tenantIdentifier:{msgInfo.Tenantdentifier},To:{msgInfo.SmsRecipient},SecurityId:{msgInfo.LogEntry.SecurityStamp}");

                                var db = TenantDbConnMgr.GetContext(msgInfo.Tenantdentifier);

                                // TODO: short urls
                                var outcome = await WhatsAppClient.Send(msgInfo.SmsRecipient, msgInfo.From, msgInfo.SmsContent);

                                if (outcome)
                                {
                                    Logger.LogInformation($"Whatsapp sent");

                                    msgInfo.LogEntry.SentOn = DateTime.Now;
                                    msgInfo.LogEntry.Status = CampaignLogStatus.Sent.ToString();

                                    db.Add(msgInfo.LogEntry);
                                    db.SaveChanges();

                                    Logger.LogInformation($"CampaignLog for Whatsapp with id: [{msgInfo.LogEntry.Id}] updated");
                                }
                                else
                                {
                                    // replace in the queue
                                    Queue.Enqueue(msgInfo);
                                    Logger.LogWarning($"Whatsapp couldn't be sent, replaced in the queue");
                                    Logger.LogInformation($"Total Pending Whatsapp: {Queue.Count()}");
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