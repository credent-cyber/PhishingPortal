using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class TrainingLog : Auditable
    {
        public int TrainingID { get; set; }
        public int ReicipientID { get; set; }
        public string TrainingType { get; set; }
        public decimal PercentCompleted { get; set; }
        public string Status { get; set; }
        public string SecurityStamp { get; set; }
        public string Url { get; set; }
        public DateTime SentOn { get; set; }
        public string UniqueID { get; set; }
        public int? CampaignLogID { get; set; }
    }
}
