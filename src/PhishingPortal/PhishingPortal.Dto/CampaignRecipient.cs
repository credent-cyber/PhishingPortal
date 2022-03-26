using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhishingPortal.Dto
{
    public class CampaignRecipient : BaseEntity
    {
        public int CampaignId { get; set; }

        public int? RecipientId { get; set; }

        [ForeignKey("RecipientId")]
        public virtual ICollection<Recipient> Recipients { get; set; }

        public int? RecipientGroupId { get; set; }

        [ForeignKey("RecipientGroupId")]
        public virtual ICollection<RecipientGroup> RecipientGroups { get; set; }
    }

}
