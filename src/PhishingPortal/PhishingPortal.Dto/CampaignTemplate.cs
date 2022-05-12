namespace PhishingPortal.Dto
{
    public class CampaignTemplate : Auditable
    {
        public string Type { get; set; }
        public bool IsHtml { get; set; }
        public string Content { get; set; }
        public int Version { get; set; }
        public  bool IsActive { get; set; }
    }

    public enum CampaignType
    {
        Email,
        Sms,
        Whatsapp
    }
}
