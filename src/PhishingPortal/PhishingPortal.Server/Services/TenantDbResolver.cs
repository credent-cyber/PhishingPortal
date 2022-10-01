using Microsoft.EntityFrameworkCore;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Services.Interfaces;

namespace PhishingPortal.Server.Services
{
    public class TenantDbResolver : ITenantDbResolver
    {

        readonly Tenant _tenant;
        readonly TenantDbContext _tenantDbCtx;

        public TenantDbResolver(ILogger<TenantDbResolver> logger, ITenantAdminRepository adminRepository, IHttpContextAccessor httpContextAccessor)
        {
            Logger = logger;

            try
            {

                var usr = httpContextAccessor?.HttpContext?.User;

                var isAuthenticated = usr?.Identity?.IsAuthenticated;

                if (isAuthenticated.HasValue && isAuthenticated.Value)
                {
                    var email = usr.Claims.FirstOrDefault(o => o.Type == "name");
                    var domain = email?.Value.Split("@")[1];

                    if (domain == null)
                        throw new InvalidOperationException("Tenant couldn't be resolved");

                    _tenant = adminRepository.GetByDomain(domain).Result;
                }
                else
                {
                    var tenantId = httpContextAccessor?.HttpContext?.Request.Query["t"];
                    
                    if (tenantId.HasValue && !string.IsNullOrEmpty(tenantId))
                    {
                        var sanitizeId = tenantId.Value.ToString().Replace("()", "");
                        _tenant = adminRepository.GetByUniqueId(sanitizeId).Result;
                    }
                        
                }

                if (Tenant == null)
                    throw new InvalidOperationException("Tenant couldn't be resolved");

                var dbSetting = Tenant.Settings.FirstOrDefault(o => o.Key == TenantData.Keys.ConnString);

                if (dbSetting == null)
                    throw new InvalidDataException("Db connection string not specified");

                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

                if (Tenant.DatabaseOption == DbOptions.SqlLite)
                {
                    optionsBuilder.UseSqlite(dbSetting.Value);
                }
                else if (Tenant.DatabaseOption == DbOptions.MySql)
                {
                    optionsBuilder.UseMySql(dbSetting.Value, ServerVersion.AutoDetect(dbSetting.Value));
                }
                else if(Tenant.DatabaseOption == DbOptions.MsSql)
                {
                    optionsBuilder.UseSqlServer(dbSetting.Value, opts => { });
                }
                else
                {
                    throw new NotImplementedException("Db provider not implement");
                }

                _tenantDbCtx = new TenantDbContext(optionsBuilder.Options);
            }
            catch (Exception)
            {
                Logger.LogCritical("Error initializing tenant base controller");
                throw;
            }

        }

        public ILogger Logger { get; }

        public Tenant Tenant => _tenant;

        public TenantDbContext TenantDbCtx => _tenantDbCtx;
    }
}
