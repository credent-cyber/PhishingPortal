using PhishingPortal.Dto.Subscription;

namespace PhishingPortal.UI.Blazor.Services
{
    public interface ILicenseService
    {
        Task<(bool, AccessMode)> IsAccessible(AppModules module);
    }
}
