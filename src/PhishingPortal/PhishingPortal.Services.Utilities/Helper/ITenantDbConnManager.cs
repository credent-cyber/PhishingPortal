using Microsoft.EntityFrameworkCore;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;

namespace PhishingPortal.Services.Utilities.Helper
{
    public interface ITenantDbConnManager : IDisposable
    {
        TenantDbContext GetContext(string tenantUniqueId);
        DbContextOptionsBuilder<TenantDbContext> SetupDbContextBuilder(Tenant tenant, string connString);
    }
}