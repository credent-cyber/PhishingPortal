using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhishingPortal.Dto
{
    public class TrainingQuizAnswer : BaseEntity
    {
        [ForeignKey("TrainingQuizId")]
        public int TrainingQuizId { get; set; }
        public string AnswerText { get; set; }
        public int OrderNumber { get; set; }
        public bool IsCorrect { get; set; }

    }
}