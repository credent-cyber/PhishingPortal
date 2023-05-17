using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class TrainingCampaignMapping : BaseEntity
    {
        public int TrainingId { get; set; }
        public int CampaignId { get; set; }

        [ForeignKey("TrainingId")]
        public virtual Training Training { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }
    }
}
