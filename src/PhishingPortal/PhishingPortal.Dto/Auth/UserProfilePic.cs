using System.Collections.Generic;

namespace PhishingPortal.Dto.Auth
{
    public class UserProfilePic: BaseEntity
    {
        public string User { get; set; } = string.Empty;
        public string ProfileUrl { get; set; } = string.Empty;
        public string BackgroundUrl { get; set; } = string.Empty;
        
    }
}

