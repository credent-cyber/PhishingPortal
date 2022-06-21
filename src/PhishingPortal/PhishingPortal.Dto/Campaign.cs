using Newtonsoft.Json;
using PhishingPortal.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class Campaign : Auditable
    {
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Category { get; set; }
        
        public CampaignStateEnum State { get; set; }
        
        public bool IsActive { get; set; }
        
        public virtual CampaignDetail Detail { get; set; }

        public int CampaignScheduleId { get; set; }

        [ForeignKey("CampaignScheduleId")]
        public virtual CampaignSchedule Schedule { get; set; }
        
        public string ReturnUrl { get; set; }

        [Required]
        [RegularExpression(pattern: AppConfig.EmailRegex, ErrorMessage = "Please specify a valid email id")]
        public string FromEmail { get; set; }
    }
}
