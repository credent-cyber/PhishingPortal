using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class TrainingQuizQuestion : BaseEntity
    {
        [Required]
        public int TrainingQuizId { get; set; }
        [Required]
        public string Question { get; set; }
        [NotMapped]
        public int QuestionNumber { get; set; }
        public string AnswerType { get; set; }
        public int OrderNumber { get; set; }
        [Range(0, 100, ErrorMessage = "Weightage must be between 0 and 100.")]
        public decimal Weightage { get; set; }
        public bool IsActive { get; set; }
        public virtual List<TrainingQuizAnswer> TrainingQuizAnswer { get; set; }

        public TrainingQuizQuestion()
        {
            TrainingQuizAnswer = new List<TrainingQuizAnswer>();
        }
    }

    public class TrainingQuiz : BaseEntity
    {
        public string Name { get; set; }
    }

    public class TrainingQuizResult
    {
        public TrainingQuiz Quiz { get; set; }
        public IEnumerable<TrainingQuizQuestion> Questions { get; set; }
    }
}
