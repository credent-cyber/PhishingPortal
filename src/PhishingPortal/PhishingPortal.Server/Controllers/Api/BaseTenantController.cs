using Microsoft.EntityFrameworkCore;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;

namespace PhishingPortal.Server.Controllers.Api
{
    public class BaseTenantController : BaseApiController
    {
        public TenantDbContext TenantDbCtx { get; private set; }
        public Tenant Tenant { get; private set; }
        public ITenantAdminRepository AdminRepository { get; }
        public IHttpContextAccessor HttpContextAccessor { get; }

        public BaseTenantController(ILogger logger, ITenantAdminRepository adminRepository, IHttpContextAccessor httpContextAccessor)
            : base(logger)
        {

            try
            {
                AdminRepository = adminRepository;
                HttpContextAccessor = httpContextAccessor;

                // TODO : refactor this 
                var usr = httpContextAccessor?.HttpContext?.User;
                var email = usr?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                //var tenantUid = usr?.FindFirst("tenant");
                var domain = email?.Value.Split("@")[1];

                #region create db context for tenant

                if (domain == null)
                    throw new InvalidOperationException("Tenant couldn't be resolved");

                Tenant = adminRepository.GetByDomain(domain).Result;

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

                // TODO: handle other database providers (mysql, postgresql)

                TenantDbCtx = new TenantDbContext(optionsBuilder.Options);
            }
            catch (Exception)
            {
                Logger.LogCritical("Error initializing tenant base controller");
                throw;
            }
            #endregion

        }

    }
}
