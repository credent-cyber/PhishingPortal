using PhishingPortal.DataContext;
using PhishingPortal.Dto.Subscription;

namespace PhishingPortal.Services.Notification
{
    public interface ILicenseEnforcementService
    {
        bool HasValidLicense(TenantDbContext tenandDbCtx, AppModules module);
    }
}