using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class TrainingCompaignMapping : BaseEntity
    {
        public int TrainingId { get; set; }
        public int CampaignId { get; set; }
    }
}
