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
        public string TrainingID { get; set; }
        public string UserID { get; set; }
        public decimal PercentCompleted { get; set; }
        public TrainingStatus Status { get; set; }

    }
    public enum TrainingStatus
    {
        Assigned,
        InProgress,
        Completed,

    }
}
