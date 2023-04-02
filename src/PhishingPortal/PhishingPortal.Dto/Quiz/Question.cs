using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto.Quiz
{
    public class Question
    {
        public TrainingQuiz TrainingQuiz { get; set; }
        public List<QuestionOption> Options { get; set; }
        public QuestionOption SelectedOption { get; set; }
    }

    public class QuestionOption
    {
        public TrainingQuizAnswer Option { get; set; }
        public bool Selected { get; set; }
    }

    public class Scores
    {
        public bool Pass { get; set; }
        public decimal Score { get; set; }
    }

    public enum TrainingStatus
    {
        Watched,
        Failed,
        Completed,
        Continued,
        Pending
    }
}
