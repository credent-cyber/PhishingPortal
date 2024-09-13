using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto.Subscription
{

    public enum AppModules
    {
        EmailCampaign,
        SmsCampaign,
        WhatsAppCampaign,
        TrainingCampaign
    }

    public class SubscriptionInfo
    {
        public LicenseTypes SubscriptionType { get; set; }

        public List<AppModules> Modules { get; set; } = new List<AppModules>();

        [Range(0, int.MaxValue)]
        public int TransactionCount { get; set; } = 1;

        [Range(0, int.MaxValue)]
        public int AllowedUserCount { get; set; } = 50;

        [Required]
        public DateTime ExpiryInUTC { get; set; } = DateTime.UtcNow.AddDays(30);

        [Required]
        public string TenantIdentifier { get; set; }

        [EmailAddress]
        public string TenantEmail { get; set; }
    }

}
