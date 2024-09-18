using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using PhishingPortal.Dto.Subscription;
using PhishingPortal.Licensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification
{
    public class LicenseEnforcementService : ILicenseEnforcementService
    {
        public LicenseEnforcementService(ILogger<LicenseEnforcementService> logger, ILicenseProvider licenseProvider)
        {
            Logger = logger;
            LicenseProvider = licenseProvider;
        }

        public bool HasValidLicense(TenantDbContext tenandDbCtx, AppModules module)
        {
            // find the subscription
            // find the current stats
            var licenseData = tenandDbCtx.Settings
                .Where(o => o.Key == TenantData.Keys.License || o.Key == TenantData.Keys.PublicKey).ToList();

            var licenseKey = licenseData.FirstOrDefault(o => o.Key == TenantData.Keys.License)?.Value ?? string.Empty;
            var publicKey = licenseData.FirstOrDefault(o => o.Key == TenantData.Keys.PublicKey)?.Value ?? string.Empty;
            var subscriptionInfo = LicenseProvider.GetSubscriptionInfo(licenseKey, publicKey);
            var licenseDate = DateTime.Now;

            if (subscriptionInfo == null)
                return false;

            var licenseSetting = licenseData.FirstOrDefault(o => o.Key == TenantData.Keys.License);
            licenseDate = licenseSetting?.ModifiedOn ?? licenseSetting?.CreatedOn ?? DateTime.MinValue;

            var campaignCounts = tenandDbCtx.Campaigns
                .Where(o => o.ModifiedOn > licenseDate && (o.State == CampaignStateEnum.Published || o.State == CampaignStateEnum.Completed || o.State == CampaignStateEnum.InProgress))
                .GroupBy(o => o.Detail.Type).Select(o => new { Type = o.Key, Count = o.Count() });

            var isValid = false;

            switch (module)
            {
                case AppModules.EmailCampaign:

                    var cnt = campaignCounts.FirstOrDefault(o => o.Type == CampaignType.Email)?.Count;

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.EmailCampaign && subscriptionInfo.TransactionCount > cnt)))
                        isValid = true;
                    break;

                case AppModules.SmsCampaign:

                    var sms = campaignCounts.FirstOrDefault(o => o.Type == CampaignType.Sms)?.Count;

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.EmailCampaign && subscriptionInfo.TransactionCount > sms)))
                        isValid = true;
                    break;

                case AppModules.WhatsAppCampaign:

                    var wapp = campaignCounts.FirstOrDefault(o => o.Type == CampaignType.Whatsapp)?.Count;

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.EmailCampaign && subscriptionInfo.TransactionCount > wapp)))
                        isValid = true;
                    break;

                case AppModules.TrainingCampaign:

                    var trainings = tenandDbCtx.Training
                        .Count(o => o.CreatedOn > licenseDate && (o.State == TrainingState.Published || o.State == TrainingState.Completed || o.State == TrainingState.InProgress));

                    if (subscriptionInfo.Modules.Any(o => (o == AppModules.TrainingCampaign && subscriptionInfo.TransactionCount > trainings)))
                        isValid = true;

                    break;

                default:

                    break;
            }

            return isValid;
        }

        public ILogger<LicenseEnforcementService> Logger { get; }
        public ILicenseProvider LicenseProvider { get; }
    }
}
