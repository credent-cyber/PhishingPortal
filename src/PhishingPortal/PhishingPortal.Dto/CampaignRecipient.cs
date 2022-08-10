using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhishingPortal.Dto
{
    public class CampaignRecipient : BaseEntity
    {
        public int CampaignId { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [ForeignKey("RecipientId")]
        public int? RecipientId { get; set; }
        public virtual Recipient Recipient { get; set; }

        public int? RecipientGroupId { get; set; }

        [ForeignKey("RecipientGroupId")]
        public virtual RecipientGroup RecipientGroup { get; set; }


    }

}
