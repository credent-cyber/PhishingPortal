using System.ComponentModel.DataAnnotations;

namespace PhishingPortal.Dto
{
    public class TenantConfirmationRequest
    {
        [Required]
        public string Url { get; set; }
        
        [Required]
        public string UniqueId { get; set; }
        
        [Required]
        public string RegisterationHash { get; set; }

    }
}
