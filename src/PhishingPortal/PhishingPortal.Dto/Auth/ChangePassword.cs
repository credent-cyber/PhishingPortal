using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PhishingPortal.Dto.Auth
{
    public class ChangePassword
    {
        //[Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Confirm password should match with the New Password")]
        public string ConfirmedPassword { get; set; }
    }
}
