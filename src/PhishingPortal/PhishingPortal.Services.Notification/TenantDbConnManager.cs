using Microsoft.EntityFrameworkCore;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;

namespace PhishingPortal.Services.Notification
{
    public class TenantDbConnManager : ITenantDbConnManager
    {
        private readonly Dictionary<string, TenantDbContext> _dicConnections;
        private readonly ILogger<TenantDbConnManager> _logger;
        private readonly CentralDbContext centralDbContext;

        private string _sqlLiteDbPath { get; } = "./../PhishingPortal.Server/App_Data";
        private bool _useSqlLite = false;

        public TenantDbConnManager(ILogger<TenantDbConnManager> logger, IConfiguration config, CentralDbContext centralDbContext)
        {
            _dicConnections = new Dictionary<string, TenantDbContext>();

            if(config != null)
            {
                _useSqlLite = config.GetValue<bool>("UseSqlLite");
                _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");

                logger.LogInformation($"UserSqlLite : {_useSqlLite}");
                logger.LogInformation($"SqlLiteDbpath  : {_sqlLiteDbPath}");
            }

            this._logger = logger;
            this.centralDbContext = centralDbContext;
        }

        public TenantDbContext GetContext(string tenantUniqueId)
        {
            if (!_dicConnections.ContainsKey(tenantUniqueId))
            {
                _logger.LogInformation($"Dbcontext not found in the dictionary");

                var tenant = centralDbContext.Tenants.Include(o => o.Settings)
                            .FirstOrDefault(o => o.UniqueId == tenantUniqueId);

                if (tenant == null)
                    throw new InvalidDataException();

                var tenantData = tenant.Settings.FirstOrDefault(o => o.Key == TenantData.Keys.ConnString);

                if (tenantData == null)
                    throw new InvalidOperationException();

                var dbCtxOptions = SetupDbContextBuilder(tenant, tenantData.Value);

                var dbcontext = new TenantDbContext(dbCtxOptions.Options);

                _dicConnections[tenantUniqueId] = dbcontext;

                return dbcontext;

            }
            else
            {
                _logger.LogInformation($"Dbcontext from cache, TenantId: {tenantUniqueId}");
                return _dicConnections[tenantUniqueId];
            }

        }

        public DbContextOptionsBuilder<TenantDbContext> SetupDbContextBuilder(Tenant tenant, string connString)
        {
            _logger.LogInformation($"Setting up db context builder");
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

            if (tenant.DatabaseOption == DbOptions.SqlLite)
            {
                _logger.LogInformation($"Using sqllite");
                var cstr = connString.Replace("./App_Data", _sqlLiteDbPath);
                _logger.LogDebug($"ConnectionString: {connString}");
                optionsBuilder.UseSqlite(cstr);
            }
            else if (tenant.DatabaseOption == DbOptions.MySql)
            {
                _logger.LogInformation($"Using mysql");
                optionsBuilder.UseMySql(connString, ServerVersion.AutoDetect(connString));
            }

            else if (tenant.DatabaseOption == DbOptions.MsSql)
            {
                _logger.LogInformation($"Using mssql");
                optionsBuilder.UseSqlServer(connString, opts => { });
            }

            return optionsBuilder;
        }

        public void Dispose()
        {
            if(_dicConnections != null)
            {
                _logger.LogInformation("Disposing all tenant db connections");
                foreach(var conn in _dicConnections.Values)
                {
                    _logger.LogDebug($"{conn}");
                    conn.Dispose();
                }

                _logger.LogInformation("Disposing all tenant db connections - done");
            }

            _dicConnections?.Clear();
        }
    }
}
