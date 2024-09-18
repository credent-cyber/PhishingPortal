using PhishingPortal.Dto;
using PhishingPortal.Dto.Subscription;
using PhishingPortal.UI.Blazor.Client;

namespace PhishingPortal.UI.Blazor.Services
{
    public class LicenseService : ILicenseService
    {
        private SubscriptionInfo subscriptionInfo;
        private Dictionary<AppModules, int> _appModuleStats;

        public LicenseService(ILogger<LicenseService> logger, TenantClient tenantClient)
        {
            Logger = logger;
            TenantClient = tenantClient;

        }

        public ILogger<LicenseService> Logger { get; }
        public TenantClient TenantClient { get; }

        public bool IsAccessible(AppModules module)
        {

            subscriptionInfo = TenantClient.GetSubscription().Result;

            // Add method in TenantClient
            // TODO : we should get counts based on the date between the renewal of the subscription and the expiry

            var stats = new Dictionary<AppModules, int>
            {
                { AppModules.EmailCampaign, 1 },
                { AppModules.SmsCampaign, 0 },
                { AppModules.WhatsAppCampaign, 0 },
                { AppModules.TrainingCampaign, 0 },
            };

            if (subscriptionInfo == null)
                return false;

            var isValid = false;

            switch (module)
            {
                case AppModules.EmailCampaign:

                    var cnt = stats[AppModules.EmailCampaign];

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.EmailCampaign && subscriptionInfo.TransactionCount > cnt)))
                        isValid = true;

                    break;

                case AppModules.SmsCampaign:

                    var sms = stats[AppModules.SmsCampaign];

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.SmsCampaign && subscriptionInfo.TransactionCount > sms)))
                        isValid = true;

                    break;

                case AppModules.WhatsAppCampaign:

                    var whatsapp = stats[AppModules.WhatsAppCampaign];

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.WhatsAppCampaign && subscriptionInfo.TransactionCount > whatsapp)))
                        isValid = true;

                    break;

                case AppModules.TrainingCampaign:

                    var training = stats[AppModules.TrainingCampaign];

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.TrainingCampaign && subscriptionInfo.TransactionCount > training)))
                        isValid = true;

                    break;

                default:

                    break;
            }

            return isValid;
        }
    }
}
