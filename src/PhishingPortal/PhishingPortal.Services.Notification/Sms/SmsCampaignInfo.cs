using PhishingPortal.Dto;

namespace PhishingPortal.Services.Notification.Sms
{
    public class SmsCampaignInfo
    {
        public string Tenantdentifier { get; set; }
        public string SmsRecipient { get; set; }
        public string SmsContent { get; set; }
        public string From { get; set; }
        public string TemplateId { get; set; }
        public CampaignLog LogEntry { get; set; }

    }
}
