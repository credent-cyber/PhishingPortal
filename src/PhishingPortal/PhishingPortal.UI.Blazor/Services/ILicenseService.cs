using PhishingPortal.Dto.Subscription;

namespace PhishingPortal.UI.Blazor.Services
{
    public interface ILicenseService
    {
        bool IsAccessible(AppModules module);
    }
}