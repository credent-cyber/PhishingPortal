using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhishingPortal.Common;
using PhishingPortal.DataContext;
using PhishingPortal.Services.OnPremiseAD.ActiveDirectory;
using PhishingPortal.Services.OnPremiseAD.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.OnPremiseAD
{
    public partial class Worker : BackgroundService
    {

        public Worker(ILogger<Worker> logger,
            ILogger<SyncUsersDetails> adLogger,
            IConfiguration configuration,
            CentralDbContext centralDbContext,
            ADApplicationSetting applicationSettings,
            ITenantDbConnManager tenantDbConnManager

          )

        {
            _logger = logger;
            this.adSyncLogger = adLogger;
            _settings = applicationSettings;
            _centralDbContext = centralDbContext;
            _configuration = configuration;
            TenantDbConnManager = tenantDbConnManager;
        }
        bool _isprocessing = false;
        private readonly ILogger<SyncUsersDetails> adSyncLogger;
        readonly ILogger<Worker> _logger;
        readonly IConfiguration _configuration;
        public ITenantDbConnManager TenantDbConnManager { get; }
        readonly ADApplicationSetting _settings;
        readonly CentralDbContext _centralDbContext;
        private readonly ADApplicationSetting applicationSettings;


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

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
                    //_isprocessing = true;
                    if (tenants != null)
                    {
                        foreach (var tenant in tenants)
                        {
                            var task = await Task.Factory.StartNew(async () =>
                            {
                                try
                                {
                                    if (_settings.EnableSyncADUsersDetail)
                                    {
                                        var provider = new SyncUsersDetails(adSyncLogger, _configuration, tenant, TenantDbConnManager);                                       
                                        await provider.Sync(stoppingToken);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex.Message);
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


