using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Domain
{
    public class CampaignDetail
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string Type { get; set; }
        public int CampaignTypeDetailId { get; set; }
    }
}
