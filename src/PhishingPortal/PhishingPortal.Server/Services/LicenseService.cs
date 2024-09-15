using PhishingPortal.Dto;
using PhishingPortal.Dto.Subscription;
using PhishingPortal.Licensing;
using PhishingPortal.Repositories;

namespace PhishingPortal.Server.Services
{
    public class LicenseService : ILicenseService
    {
        private const string NoSubscription = "NO_SUBSCRIPTION";
        private string licenseKey;
        private string publicKey;

        public LicenseService(ITenantAdminRepository tenantAdmin, ILicenseProvider licenseProvider)
        {
            TenantAdmin = tenantAdmin;
            LicenseProvider = licenseProvider;

            licenseKey = string.Empty;
            publicKey = string.Empty;

        }

        public ITenantAdminRepository TenantAdmin { get; }
        public ILicenseProvider LicenseProvider { get; }

        public async Task<bool> HasValidLicense(string tenantIdentifier)
        {
            var subscription = await GetSubscription(tenantIdentifier);
            return subscription.Valid;
        }

        public async Task<(bool Valid, SubscriptionInfo? SubscriptionInfo)> GetSubscription(string tenantIdentifier)
        {
            if (licenseKey == NoSubscription)
                return (false, null);

            var tenant = await TenantAdmin.GetByUniqueId(tenantIdentifier);
            var db = TenantDbResolver.CreateTenantDbContext(tenant);

            licenseKey = db.Settings.FirstOrDefault(o => o.Key == TenantData.Keys.License)?.Value ?? string.Empty;
            publicKey = db.Settings.FirstOrDefault(o => o.Key == TenantData.Keys.PublicKey)?.Value ?? string.Empty;

            // no subscription
            if (string.IsNullOrEmpty(licenseKey) || string.IsNullOrEmpty(publicKey))
            {
                licenseKey = NoSubscription;
                return (false, null);
            }

            return LicenseProvider.Validate(licenseKey, publicKey);

        }
    }
}
