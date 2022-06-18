using PhishingPortal.DataContext;
using PhishingPortal.Dto;

namespace PhishingPortal.Server.Services
{
    public interface ITenantDbResolver
    {
        Tenant Tenant { get; }
        TenantDbContext TenantDbCtx { get; }
    }
}