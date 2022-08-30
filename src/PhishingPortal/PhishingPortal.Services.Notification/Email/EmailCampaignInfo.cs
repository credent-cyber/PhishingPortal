using PhishingPortal.Dto;

namespace PhishingPortal.Services.Notification.Email
{

    public class EmailCampaignInfo
    {
        public string Tenantdentifier { get; set; }
        public string EmailRecipients { get; set; }
        public string EmailSubject { get; set; }
        public string EmailContent { get; set; }
        public string EmailFrom { get; set; }
        public CampaignLog LogEntry { get; set; }

    }
}
