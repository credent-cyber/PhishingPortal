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

        public async Task<(bool, AccessMode)> IsAccessible(AppModules module)
        {

            subscriptionInfo = await TenantClient.GetSubscription();
            AccessMode accessMode = AccessMode.None;
            // Add method in TenantClient
            // TODO : we should get counts based on the date between the renewal of the subscription and the expiry

            var stats = new Dictionary<AppModules, int>
            {
                { AppModules.EmailCampaign, 8 },
                { AppModules.SmsCampaign, 0 },
                { AppModules.WhatsAppCampaign, 2 },
                { AppModules.TrainingCampaign, 0 },
            };

            if (subscriptionInfo == null)
                return (false, AccessMode.None);

            var isValid = false;

            var email = stats[AppModules.EmailCampaign];
            var sms = stats[AppModules.SmsCampaign];
            var whatsapp = stats[AppModules.WhatsAppCampaign];
            var training = stats[AppModules.TrainingCampaign];

            switch (module)
            {
                case AppModules.EmailCampaign:

                   // var email = stats[AppModules.EmailCampaign];
                  
                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.EmailCampaign && subscriptionInfo.TransactionCount > (email + sms + whatsapp + training))))
                    {
                        isValid = true;
                        accessMode = AccessMode.ReadWrite;
                    }
                    else
                    {
                        isValid = false;
                        accessMode = subscriptionInfo.Modules.Any(o => (o == AppModules.EmailCampaign)) ?  AccessMode.ReadOnly : AccessMode.None;
                    }

                    break;

                case AppModules.SmsCampaign:

                    //var sms = stats[AppModules.SmsCampaign];

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.SmsCampaign && subscriptionInfo.TransactionCount > (email + sms + whatsapp + training))))
                    {
                        isValid = true;
                        accessMode = AccessMode.ReadWrite;
                    }
                    else
                    {
                        isValid = false;
                        accessMode = subscriptionInfo.Modules.Any(o => (o == AppModules.SmsCampaign)) ? AccessMode.ReadOnly : AccessMode.None;
                    }

                    break;

                case AppModules.WhatsAppCampaign:

                    //var whatsapp = stats[AppModules.WhatsAppCampaign];

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.WhatsAppCampaign && subscriptionInfo.TransactionCount > (email + sms + whatsapp + training))))
                    {
                        isValid = true;
                        accessMode = AccessMode.ReadWrite;
                    }
                    else
                    {
                        isValid = false;
                        accessMode = subscriptionInfo.Modules.Any(o => (o == AppModules.WhatsAppCampaign)) ? AccessMode.ReadOnly : AccessMode.None;
                    }

                    break;

                case AppModules.TrainingCampaign:

                    //var training = stats[AppModules.TrainingCampaign];

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.TrainingCampaign && subscriptionInfo.TransactionCount > (email + sms + whatsapp + training))))
                    {
                        isValid = true;
                        accessMode = AccessMode.ReadWrite;
                    }
                    else
                    {
                        isValid = false;
                        accessMode = subscriptionInfo.Modules.Any(o => (o == AppModules.TrainingCampaign)) ? AccessMode.ReadOnly : AccessMode.None;
                    }

                    break;

                default:

                    break;
            }

            return (isValid, accessMode); ;
        }
    }
}

public enum AccessMode
{
    ReadWrite,
    ReadOnly,
    None
}
