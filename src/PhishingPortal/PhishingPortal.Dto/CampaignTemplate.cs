using System.ComponentModel.DataAnnotations;

namespace PhishingPortal.Dto
{
    public class CampaignTemplate : Auditable
    {
        [Required(ErrorMessage = "The Template Name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Template Type is required.")]
        public CampaignType? Type { get; set; }
        public bool IsHtml { get; set; }
        public bool IsActive { get; set; }
        public string Content { get; set; }
        public string Version { get; set; }
    }

    public enum CampaignType
    {
        Email = 1,
        Sms,
        Whatsapp
    }


}
