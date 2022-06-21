using Microsoft.EntityFrameworkCore;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;

namespace PhishingPortal.Services.Notification
{
    public class TenantDbConnManager : ITenantDbConnManager
    {
        private readonly Dictionary<string, TenantDbContext> _dicConnections;

        private readonly CentralDbContext centralDbContext;

        private string _sqlLiteDbPath { get; } = "./../PhishingPortal.Server/App_Data";

        public TenantDbConnManager(ILogger<TenantDbConnManager> logger, IConfiguration config, CentralDbContext centralDbContext)
        {
            _dicConnections = new Dictionary<string, TenantDbContext>();

            if(config != null)
            {
                _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");
            }

            this.centralDbContext = centralDbContext;
        }

        public TenantDbContext GetContext(string tenantUniqueId)
        {
            if (!_dicConnections.ContainsKey(tenantUniqueId))
            {
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
                return _dicConnections[tenantUniqueId];
            }

        }

        public DbContextOptionsBuilder<TenantDbContext> SetupDbContextBuilder(Tenant tenant, string connString)
        {

            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

            if (tenant.DatabaseOption == DbOptions.SqlLite)
            {
                var cstr = connString.Replace("./App_Data", _sqlLiteDbPath);
                optionsBuilder.UseSqlite(cstr);
            }
            else if (tenant.DatabaseOption == DbOptions.MySql)
            {
                throw new NotImplementedException("NO implemantation for Mssql provider");
            }

            return optionsBuilder;
        }

        public void Dispose()
        {
            if(_dicConnections != null)
            {
                foreach(var conn in _dicConnections.Values)
                {
                    conn.Dispose();
                }
            }

            _dicConnections?.Clear();
        }
    }
}
