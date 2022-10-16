using System.ComponentModel.DataAnnotations;

namespace PhishingPortal.Dto.Auth
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Confirm password should match with the New Password")]
        public string ConfirmedPassword { get; set; }
    }
}
