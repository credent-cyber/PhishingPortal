using PhishingPortal.Dto.Subscription;

namespace PhishingPortal.Server.Services
{
    public interface ILicenseService
    {
        Task<(bool Valid, SubscriptionInfo? SubscriptionInfo)> GetSubscription(string tenantIdentifier);
        Task<bool> HasValidLicense(string tenantIdentifier);
    }
}