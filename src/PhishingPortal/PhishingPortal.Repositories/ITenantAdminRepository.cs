using PhishingPortal.Dto;

namespace PhishingPortal.Repositories
{
    public interface ITenantAdminRepository
    {
        Task<Tenant> CreateTenantAsync(Tenant tenant);
    }
}