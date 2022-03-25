using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Domain
{
    public class CampaignLog
    {
        public int CampaignId { get; set; }
        public int EmployeeId { get; set; }
        public string CampignType { get; set; }
        public string SentOn { get; set; }
        public string SentBy { get; set; }
        public string Link { get; set; }
        public bool IsHit { get; set; }
        public string ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }  
}
