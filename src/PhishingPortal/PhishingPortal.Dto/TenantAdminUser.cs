using System.ComponentModel.DataAnnotations;

namespace PhishingPortal.Dto
{
    public class TenantAdminUser
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$")]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        [Required]
        public string ConfirmPassword { get; set; }
    }

}
