using PhishingPortal.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class DemoRequestor : Auditable
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [RegularExpression(pattern: AppConfig.EmailRegex, ErrorMessage = "Please specify a valid email id")]
        public string Email { get; set; }
      
        public string ContactNumber { get; set; }
        public string Company { get; set; }
        public string Messages { get; set; }
        
        public Boolean isNotified { get; set; }

    }
}
