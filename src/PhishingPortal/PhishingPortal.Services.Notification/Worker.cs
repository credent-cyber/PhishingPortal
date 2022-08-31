namespace PhishingPortal.Services.Notification
{
    using PhishingPortal.DataContext;
    using Microsoft.EntityFrameworkCore;
    using PhishingPortal.Common;
    using PhishingPortal.Services.Notification.Monitoring;
    using PhishingPortal.Services.Notification.Email;
    using PhishingPortal.Services.Notification.Helper;
    using PhishingPortal.Services.Notification.Sms;

    public class Worker : BackgroundService
    {
        class WorkerSettings
        {
            public int WaitIntervalInMinutes { get; set; }
            public WorkerSettings(IConfiguration config)
            {
                config.GetSection("WorkerSettings").Bind(this);
            }
        }

        public Worker(ILogger<Worker> logger,
            ILogger<EmailCampaignProvider> agentLogger,
            IEmailClient emailClient,
            IConfiguration configuration,
            CentralDbContext centralDbContext,
            IEmailCampaignExecutor campaignExecutor, 
            ISmsCampaignExecutor smsExecutor,
            ITenantDbConnManager tenantDbConnManager, 
            ISmsGatewayClient smsClient)
        {
            _logger = logger;
            this.providerLogger = agentLogger;
            _configuration = configuration;
            _settings = new WorkerSettings(configuration);
           
            this._emailClient = emailClient;
            _configuration = configuration;
            _centralDbContext = centralDbContext;
            this._campaignExecutor = campaignExecutor;
            this._smsExecutor = smsExecutor;
            TenantDbConnManager = tenantDbConnManager;
            SmsClient = smsClient;
        }

        readonly ILogger<Worker> _logger;
        private readonly ILogger<EmailCampaignProvider> providerLogger;
        readonly WorkerSettings _settings;
        readonly IEmailClient _emailClient;
        readonly IConfiguration _configuration;
        readonly CentralDbContext _centralDbContext;
        private readonly IEmailCampaignExecutor _campaignExecutor;
        private readonly ISmsCampaignExecutor _smsExecutor;
        bool _isprocessing = false;

        public ITenantDbConnManager TenantDbConnManager { get; }
        public ISmsGatewayClient SmsClient { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _campaignExecutor.Start();
            _smsExecutor.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                if (!_isprocessing)
                {
                    var tenants = _centralDbContext.Tenants.Include(o => o.Settings).Include(o => o.TenantDomains)
                               .Where(o => o.IsActive);

                    if (tenants == null || tenants.Count() == 0)
                    {
                        _logger.LogCritical("No active tenants found");
                    }

                    List<Task> allTasks = new List<Task>();
                    _isprocessing = true;
                    if (tenants != null)
                    {
                        foreach (var tenant in tenants)
                        {
                            var task = await Task.Factory.StartNew(async () =>
                             {
                                 try
                                 {
                                     // run email campaing for each tenant
                                     var provider = new EmailCampaignProvider(providerLogger, _emailClient, _configuration, tenant, TenantDbConnManager);
                                     provider.Subscribe(_campaignExecutor);
                                     await provider.CheckAndPublish(stoppingToken);

                                     // sms campaign executor
                                     var _smsProvider = new SmsCampaignProvider(providerLogger, SmsClient, _configuration, tenant, TenantDbConnManager);
                                     _smsProvider.Subscribe(_smsExecutor);
                                     await _smsProvider.CheckAndPublish(stoppingToken);

                                     // monitor all incoming reports on the designated mail box and update the monitoring report for each campaign log
                                     var _reportMonitor = new EmailPhishingReportMonitor(providerLogger, _configuration, tenant, TenantDbConnManager);
                                     await _reportMonitor.ProcessAsync();
                                   
                                 }
                                 catch (Exception ex)
                                 {
                                     _logger.LogCritical(ex, $"Error while executing campaign for tenant [{tenant.UniqueId}], Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                                 }
                                
                             });

                            allTasks.Add(task);
                        } 
                    }

                    if (allTasks.Count > 0)
                    {
                        await Task.WhenAll(allTasks);
                    }

                    _isprocessing = false;

                    await Task.Delay(_settings.WaitIntervalInMinutes * 1000 * 60, stoppingToken); 
                }
                else
                {
                    _logger.LogInformation($"Skipping this iteration as already processing");
                }
            }

            _campaignExecutor.Stop();
        }
    }
}