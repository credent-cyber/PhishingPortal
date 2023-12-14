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

        public async Task<ApiResponse<Tenant>> CreateAsync(Tenant tenant)
        {
            var response = new ApiResponse<Tenant>();
            try
            {
                response = await TenantAdminRepo.CreateTenantAsync(tenant);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;

            }

            return response;
        }
    }

}
