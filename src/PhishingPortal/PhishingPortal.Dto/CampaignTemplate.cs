﻿namespace PhishingPortal.Dto
{
    public class CampaignTemplate : Auditable
    {
        public string Name { get; set; }
        public CampaignType Type { get; set; }
        public bool IsHtml { get; set; }
        public string Content { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
    }

    public enum CampaignType
    {
        Email = 1,
        Sms,
        Whatsapp
    }
}
