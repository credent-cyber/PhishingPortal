using System.ComponentModel.DataAnnotations;

namespace PhishingPortal.Dto.Auth
{
    public class ForgetPasswordRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
    }
}
