using System;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class CampaignDetail : Auditable
    {
        public int CampaignId { get; set; }
        public string Type { get; set; }
        public int TemplateId { get; set; }
    }

}
