using PhishingPortal.Dto;

namespace PhishingPortal.Services.Notification.Whatsapp
{
    public class WhatsappCampaignInfo
    {
        public string Tenantdentifier { get; set; }
        public string SmsRecipient { get; set; }
        public string SmsContent { get; set; }
        public string From { get; set; }
        public CampaignLog LogEntry { get; set; }

    }
}
