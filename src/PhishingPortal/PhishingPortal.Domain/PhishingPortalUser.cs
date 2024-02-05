namespace PhishingPortal.Domain
{
    using Microsoft.AspNetCore.Identity;

    public class PhishingPortalUser : IdentityUser {

        public string ActiveDirectoryUsername { get; set; } = string.Empty;

        public bool IsOnPremADUser { get; set; }

    }

}