using PhishingPortal.Dto;

namespace PhishingPortal.Repositories
{
    public interface ITenantAdminRepository
    {
        Task<List<Tenant>> GetAllAsync(int pageIndex = 0, int pageSize = 10);

        Task<Tenant> CreateTenantAsync(Tenant tenant);

        Task<bool> ProvisionAsync(int tenantId, string connectionString);

        Task<Tenant> GetByUniqueId(string uniqueId);

        Task<Tenant> GetByDomain(string domain);

        Task<Tenant> ConfirmRegistrationAsync(string uniqueId, string hash, string link);

        Task<Tenant> ConfirmDomainAsync(DomainVerificationRequest domain);

        Task<DemoRequestor> UpsertDemoRequestor(DemoRequestor demoRequestor);

    }
}