namespace PhishingPortal.Services.Notification
{
    using PhishingPortal.DataContext;
    using Microsoft.EntityFrameworkCore;
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
            IEmailSender emailClient,
            IConfiguration configuration,
            CentralDbContext centralDbContext,
            IEmailCampaignExecutor campaignExecutor, ITenantDbConnManager tenantDbConnManager)
        {
            _logger = logger;
            this.providerLogger = agentLogger;
            _configuration = configuration;
            _settings = new WorkerSettings(configuration);
           
            this._emailClient = emailClient;
            _configuration = configuration;
            _centralDbContext = centralDbContext;
            this._campaignExecutor = campaignExecutor;
            TenantDbConnManager = tenantDbConnManager;
        }

        readonly ILogger<Worker> _logger;
        private readonly ILogger<EmailCampaignProvider> providerLogger;
        readonly WorkerSettings _settings;
        readonly IEmailSender _emailClient;
        readonly IConfiguration _configuration;
        readonly CentralDbContext _centralDbContext;
        private readonly IEmailCampaignExecutor _campaignExecutor;
        bool _isprocessing = false;

        public ITenantDbConnManager TenantDbConnManager { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _campaignExecutor.Start();
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
                    foreach (var tenant in tenants)
                    {
                        var task = Task.Factory.StartNew(async () =>
                         {
                             try
                             {
                                 var provider = new EmailCampaignProvider(providerLogger, _emailClient, _configuration, tenant, TenantDbConnManager);
                                 provider.Subscribe(_campaignExecutor);
                                 await provider.CheckAndPublish(stoppingToken);
                             }
                             catch (Exception ex)
                             {
                                 _logger.LogCritical(ex, $"Error while executing campaign for tenant [{tenant.UniqueId}], Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                             }

                             await Task.CompletedTask;
                         });

                        allTasks.Add(task);
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