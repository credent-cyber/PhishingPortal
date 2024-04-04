namespace PhishingPortal.Services.Notification
{
    using PhishingPortal.DataContext;
    using Microsoft.EntityFrameworkCore;
    using PhishingPortal.Common;
    using PhishingPortal.Services.Notification.Monitoring;
    using PhishingPortal.Services.Notification.Email;
    using PhishingPortal.Services.Notification.Helper;
    using PhishingPortal.Services.Notification.Sms;
    using PhishingPortal.Services.Notification.Whatsapp;
    using PhishingPortal.Services.Notification.RequestMonitor;
    using PhishingPortal.Services.Notification.Trainings;
    using PhishingPortal.Services.Notification.EmailTemplate;
    using PhishingPortal.Services.Notification.Email.AppNotifications;
    using PhishingPortal.Services.Notification.UrlShortner;
    using PhishingPortal.Services.Notification.WeeklySummaryReport;
    using PhishingPortal.Dto;

    public partial class Worker : BackgroundService
    {

        public Worker(ILogger<Worker> logger,
            ILogger<EmailCampaignProvider> agentLogger,
            ILogger<TrainingProvider> trainingLogger,
            IEmailClient emailClient,
            IConfiguration configuration,
            CentralDbContext centralDbContext,
            IEmailCampaignExecutor campaignExecutor,
            ISmsCampaignExecutor smsExecutor,
            IWhatsappCampaignExecutor whatsappCampaignExecutor,
            ITenantDbConnManager tenantDbConnManager,
            ISmsGatewayClient smsClient,
            IDemoRequestHandler demoRequestHandler,
            IWhatsappGatewayClient waClient,
            ITrainingExecutor trainingExecutor,
            IEmailTemplateProvider emailTemplateProvider,
            IAppEventNotifier appEventNotifier,
            IUrlShortner urlShortner,
            IWeeklyReportExecutor weeklyReportExecutor,
            ApplicationSettings applicationSettings
            )



        {
            _logger = logger;
            this.providerLogger = agentLogger;
            this.TrainingProviderLogger = trainingLogger;
            _configuration = configuration;
            _settings = applicationSettings;
            this._emailClient = emailClient;
            _configuration = configuration;
            _centralDbContext = centralDbContext;
            this._campaignExecutor = campaignExecutor;
            this._smsExecutor = smsExecutor;
            this._whatsappCampaignExecutor = whatsappCampaignExecutor;
            TenantDbConnManager = tenantDbConnManager;
            SmsClient = smsClient;
            WaClient = waClient;
            this._demoRequestHandler = demoRequestHandler;
            this._trainingExecutor = trainingExecutor;
            this._emailTemplateProvider = emailTemplateProvider;
            this._appEventNotifier = appEventNotifier;
            this.applicationSettings = applicationSettings;
            this.urlShortner = urlShortner;
            this._weeklyReportExecutor = weeklyReportExecutor;
        }

        readonly ILogger<Worker> _logger;
        private readonly ILogger<EmailCampaignProvider> providerLogger;
        private readonly ILogger<SmsCampaignProvider> smsProviderLogger;
        private readonly ILogger<WhatsappCampaignProvider> whatsappProviderLogger;
        private readonly ILogger<TrainingProvider> TrainingProviderLogger;
        private readonly ILogger<WeeklyReportProvider> WeeklyReportLogger;
        readonly ApplicationSettings _settings;
        readonly IEmailClient _emailClient;
        readonly IConfiguration _configuration;
        readonly CentralDbContext _centralDbContext;
        private readonly IEmailCampaignExecutor _campaignExecutor;
        private readonly ISmsCampaignExecutor _smsExecutor;
        private readonly IWhatsappCampaignExecutor _whatsappCampaignExecutor;
        private readonly IWeeklyReportExecutor _weeklyReportExecutor;
        bool _isprocessing = false;
        private readonly IDemoRequestHandler _demoRequestHandler;
        private readonly ITrainingExecutor _trainingExecutor;
        private readonly IEmailTemplateProvider _emailTemplateProvider;
        private readonly IAppEventNotifier _appEventNotifier;
        private readonly ApplicationSettings applicationSettings;

        public ITenantDbConnManager TenantDbConnManager { get; }
        public IUrlShortner urlShortner { get; }
        public ISmsGatewayClient SmsClient { get; }
        public IWhatsappGatewayClient WaClient { get; }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_settings.EnableDemoRequestHandler)
                _demoRequestHandler.Start();

            if (_settings.EnableEmailCampaign)
                _campaignExecutor.Start();

            if (_settings.EnableSmsCampaign)
                _smsExecutor.Start();

            if (_settings.EnableWhatsappCampaign)
                _whatsappCampaignExecutor.Start();

            if (_settings.EnableTrainingProvider)
                _trainingExecutor.Start();

            if (_settings.EnableWeeklyReport)
                _weeklyReportExecutor.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                if (!_isprocessing)
                {

                    // notify critical application errors via email
                    await _appEventNotifier.CheckAndNotifyErrors();

                    var tenants = _centralDbContext.Tenants.Include(o => o.Settings).Include(o => o.TenantDomains)
                               .Where(o => o.IsActive);
                    var weeklyReport = _centralDbContext.WeeklyReport;

                    if (tenants == null || tenants.Count() == 0)
                    {
                        _logger.LogCritical("No active tenants found");
                    }

                    List<Task> allTasks = new List<Task>();

                    _logger.LogInformation($"Worker set _isProcessing=true, for this cycle");
                    _isprocessing = true;
                    if (tenants != null)
                    {
                        foreach (var tenant in tenants)
                        {
                            //var task = await Task.Factory.StartNew(async () =>
                            var task = Task.Run(async () =>
                                {
                                    try
                                    {
                                        var currentDayOfWeek = DateTime.Today.DayOfWeek;
                                        // run email campaing for each tenant
                                        if (_settings.EnableEmailCampaign)
                                        {
                                            var provider = new EmailCampaignProvider(providerLogger, _emailClient, _configuration, tenant, TenantDbConnManager);
                                            provider.Subscribe(_campaignExecutor);
                                            await provider.CheckAndPublish(stoppingToken);

                                        }

                                        //// sms campaign executor
                                        if (_settings.EnableSmsCampaign)
                                        {
                                            var _smsProvider = new SmsCampaignProvider(smsProviderLogger, SmsClient, _configuration, tenant, TenantDbConnManager);
                                            _smsProvider.Subscribe(_smsExecutor);
                                            await _smsProvider.CheckAndPublish(stoppingToken);
                                        }

                                        // whatsapp provider 
                                        if (_settings.EnableWhatsappCampaign)
                                        {
                                            var _waProvider = new WhatsappCampaignProvider(whatsappProviderLogger, WaClient, _configuration, tenant, TenantDbConnManager, urlShortner);
                                            _waProvider.Subscribe(_whatsappCampaignExecutor);
                                            await _waProvider.CheckAndPublish(stoppingToken);
                                        }

                                        //training provider
                                        if (_settings.EnableTrainingProvider)
                                        {
                                            var trainingProvider = new TrainingProvider(TrainingProviderLogger, _emailClient, _configuration, tenant, TenantDbConnManager, _emailTemplateProvider);
                                            trainingProvider.Subscribe(_trainingExecutor);
                                            await trainingProvider.CheckAndPublish(stoppingToken);
                                        }

                                        //// monitor all incoming reports on the designated mail box and update the monitoring report for each campaign log
                                        if (_settings.EnableReportingMonitor)
                                        {
                                            var _reportMonitor = new EmailPhishingReportMonitor(providerLogger, _configuration, tenant, TenantDbConnManager);
                                            await _reportMonitor.ProcessAsync();
                                        }

                                        //Weekly Summary Report provider
                                        if (_settings.EnableWeeklyReport && currentDayOfWeek == DayOfWeek.Saturday)
                                        {
                                            var existingReport = weeklyReport.FirstOrDefault(x => x.Tenant == tenant.UniqueId);

                                            if (existingReport == null || !existingReport.IsReportSent)
                                            {
                                                if (existingReport == null)
                                                {
                                                    // If no existing report found, create a new one
                                                    var newWeeklyReport = new WeeklyReport
                                                    {
                                                        Tenant = tenant.UniqueId,
                                                        CreatedOn = DateTime.Now,
                                                        IsReportSent = false
                                                    };

                                                    // Add the new report to the database context
                                                    _centralDbContext.WeeklyReport.Add(newWeeklyReport);
                                                }
                                                else
                                                {
                                                    // If an existing report is found but it's not sent yet, update the report
                                                    existingReport.ModifiedOn = DateTime.Now;
                                                    existingReport.IsReportSent = true;
                                                    _centralDbContext.Update(existingReport);
                                                }

                                                await _centralDbContext.SaveChangesAsync();

                                                // Execute the weekly report provider logic
                                                var provider = new WeeklyReportProvider(WeeklyReportLogger, _emailClient, _configuration, tenant, TenantDbConnManager);
                                                provider.Subscribe(_weeklyReportExecutor);
                                                await provider.CheckAndPublish(stoppingToken);
                                            }
                                        }


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
                    _logger.LogInformation($"Worker reset _isProcessing=false, for the next cycle");
                    _isprocessing = false;
                    _demoRequestHandler.Execute();

                    await Task.Delay(_settings.WaitIntervalInMinutes * 1000 * 60, stoppingToken);
                }
                else
                {
                    _logger.LogInformation($"Skipping this iteration as already processing");
                }
            }

            _campaignExecutor.Stop();
            _trainingExecutor.Stop();
        }

    }
}