using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class Training : Auditable
    {
        [Required]
        public string TrainingName { get; set; }
        public bool IsActive { get; set; }
        public string Content { get; set; }
        [Required]
        public string TrainingCategory { get; set; }
        public string TrainingVideo { get; set; }
        public TrainingState State { get; set; }

        public int TrainingScheduleId { get; set; }

        [ForeignKey("TrainingScheduleId")]
        public virtual TrainingSchedule TrainingSchedule { get; set; }
    }

    public enum TrainingState
    {
        Draft,
        Published,
        InProgress,
        Completed,
        Aborted,
        Faulted
    }
}
