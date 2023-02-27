using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class TrainingRecipients : BaseEntity
    {
        public int TrainingId { get; set; }

        [ForeignKey("TrainingId")]
        public virtual Training Training { get; set; }

        [ForeignKey("RecipientId")]
        public int? RecipientId { get; set; }
        public virtual Recipient AllTrainingRecipient { get; set; }

        public int? RecipientGroupId { get; set; }

        [ForeignKey("RecipientGroupId")]
        public virtual RecipientGroup RecipientGroup { get; set; }

    }
}
