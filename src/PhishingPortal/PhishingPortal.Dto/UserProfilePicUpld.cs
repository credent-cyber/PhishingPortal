using PhishingPortal.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class UserProfilePicUpld : BaseEntity
    {
        [NotMapped]
        public bool removebg { get; set; } = false;
        [NotMapped]
        public bool removePp { get; set; } = false;
        public string Email { get; set; }   = string.Empty;
        public byte[]? ProfileImage { get; set; }
        public byte[]? BackgroundImage { get; set; }

    }
}

