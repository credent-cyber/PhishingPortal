using PhishingPortal.Dto;

namespace PhishingPortal.Repositories
{
    public interface ITenantAdminRepository
    {
        Task<List<Tenant>> GetAllAsync(int pageIndex = 0, int pageSize = 10);

        //Task<Tenant> CreateTenantAsync(Tenant tenant);
        Task<ApiResponse<Tenant>> CreateTenantAsync(Tenant tenant);

        Task<bool> ProvisionAsync(int tenantId, string connectionString);

        Task<Tenant> GetByUniqueId(string uniqueId);
        
        Task<(bool,string)> DeleteTenantByUniqueId(string uniqueId);

        Task<Tenant> GetByDomain(string domain);

        Task<Tenant> ConfirmRegistrationAsync(string uniqueId, string hash, string link);

        Task<Tenant> ConfirmDomainAsync(DomainVerificationRequest domain);

        Task<string> UpsertDemoRequestor(DemoRequestor demoRequestor);
        Task<TenantDomain> UpsertTenantDomain(TenantDomain domain);
        Task<IEnumerable<TenantDomain>> GetDomains(int tenantId);
        Task<TenantDomain> VerifyDomain(TenantDomain domain);
        Task<bool> DeleteDomain(int id);
    }
}