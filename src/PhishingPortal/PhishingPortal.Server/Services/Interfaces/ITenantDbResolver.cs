using PhishingPortal.DataContext;
using PhishingPortal.Dto;

namespace PhishingPortal.Server.Services.Interfaces
{
    public interface ITenantDbResolver
    {
        Tenant Tenant { get; }
        TenantDbContext TenantDbCtx { get; }
    }
}