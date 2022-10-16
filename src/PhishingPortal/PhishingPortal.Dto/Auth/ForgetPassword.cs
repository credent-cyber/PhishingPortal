using System.ComponentModel.DataAnnotations;

namespace PhishingPortal.Dto.Auth
{
    public class ForgetPasswordRequest
    {
        [Required]
        public string Email { get; set; }
    }
}
