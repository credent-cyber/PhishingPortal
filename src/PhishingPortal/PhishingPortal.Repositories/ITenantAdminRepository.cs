using PhishingPortal.Dto;

namespace PhishingPortal.Repositories
{
    public interface ITenantAdminRepository
    {
        Task<List<Tenant>> GetAllAsync(int pageIndex = 0, int pageSize = 10);
        Task<Tenant> CreateTenantAsync(Tenant tenant);

        Task<bool> ProvisionAsync(int tenantId, string connectionString);
    }
}