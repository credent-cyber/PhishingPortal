using Microsoft.Extensions.Hosting;
using PhishingPortal.DataContext;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PhishingPortal.Services.Utilities.Helper;
using PhishingPortal.Services.Utilities.WeeklyReport;

namespace PhishingPortal.Services.Utilities
{
    public partial class Worker : BackgroundService
    {

        public Worker(ILogger<Worker> logger,
            IEmailClient emailClient,
            IConfiguration configuration,
            IWeeklySummaryReports weeklySummaryReports,
            CentralDbContext centralDbContext,
            ApplicationSettings applicationSettings
            )



        {
            _logger = logger;
            _configuration = configuration;
            _settings = applicationSettings;
            this._emailClient = emailClient;
            this._weeklySummaryReports = weeklySummaryReports;
            _configuration = configuration;
            _centralDbContext = centralDbContext;

        }

        readonly ILogger<Worker> _logger;
        readonly ApplicationSettings _settings;
        readonly IEmailClient _emailClient;
        readonly IConfiguration _configuration;
        readonly IWeeklySummaryReports _weeklySummaryReports;
        readonly CentralDbContext _centralDbContext;
        bool _isprocessing = false;
        private readonly ApplicationSettings applicationSettings;

        public ITenantDbConnManager TenantDbConnManager { get; }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            if (_settings.EnableWeeklyReportService)
                _weeklySummaryReports.Start();

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
                                        // run email campaing for each tenant
                                        if (_settings.EnableWeeklyReportService)
                                        {
                                            //var provider = new EmailCampaignProvider(providerLogger, _emailClient, _configuration, tenant, TenantDbConnManager);
                                            //provider.Subscribe(_campaignExecutor);
                                            //await provider.CheckAndPublish(stoppingToken);

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

                    await Task.Delay(_settings.WaitIntervalInMinutes * 1000 * 60, stoppingToken);
                }
                else
                {
                    _logger.LogInformation($"Skipping this iteration as already processing");
                }
            }

        }

    }
}