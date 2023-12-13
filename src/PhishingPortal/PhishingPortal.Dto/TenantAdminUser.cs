using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class TenantAdminUser
{
    [Required]
    [RegularExpression("^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$")]
    public string Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "The password must be at least 6 characters long.")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).{6,}$", ErrorMessage = "The password must include at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
    public string Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    public string TenantUniqueId { get; set; }
}
