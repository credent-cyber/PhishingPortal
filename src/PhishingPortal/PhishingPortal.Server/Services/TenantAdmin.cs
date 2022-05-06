using PhishingPortal.Core;
using PhishingPortal.Dto;
using PhishingPortal.Repositories;

namespace PhishingPortal.Server.Services
{
    public class TenantAdmin : ITenantAdmin
    {

        public TenantAdmin(ILogger<TenantAdmin> logger, ITenantAdminRepository tenantAdminRepo)
        {
            Logger = logger;
            TenantAdminRepo = tenantAdminRepo;
        }

        public ILogger<TenantAdmin> Logger { get; }
        public ITenantAdminRepository TenantAdminRepo { get; }

        public async Task<Tenant> CreateAsync(Tenant tenant)
        {
            try
            {
                tenant = await TenantAdminRepo.CreateTenantAsync(tenant);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;

            }

            return tenant;
        }
    }
}
