using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{

    public class CampaignDetail : Auditable
    {
        public int CampaignId { get; set; }

        [Required]
        public CampaignType Type { get; set; }

        [Required]
        public int CampaignTemplateId { get; set; }

        public virtual CampaignTemplate Template { get; set; }
    }

}
