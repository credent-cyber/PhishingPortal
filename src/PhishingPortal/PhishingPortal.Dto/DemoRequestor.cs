using PhishingPortal.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhishingPortal.Dto
{
    public class DemoRequestor : Auditable
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [RegularExpression(pattern: "^(?!.*@(gmail|yahoo|outlook)\\.)[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Please provide your company Email!")]
        [EmailAddress]
        //[RegularExpression(pattern: AppConfig.EmailRegex, ErrorMessage = "Please specify a valid email id")]
        public string Email { get; set; }

        public string ContactNumber { get; set; }
        [Required]
        public string Company { get; set; }
        public string Messages { get; set; }

        public Boolean isNotified { get; set; }

    }
}
