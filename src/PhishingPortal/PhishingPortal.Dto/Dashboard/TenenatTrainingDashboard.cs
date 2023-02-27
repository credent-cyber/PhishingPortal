using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto.Dashboard
{

    public class MonthlyTrainingBarChart
    {
        public string Title { get; set; }
        public int Year { get; set; }
        public List<MonthlyTrainingReportData> MonthwiseTrainingEntry { get; set; }
    }

    public class MonthlyTrainingReportData
    {
        public Months Month { get; set; }
        public int TotalTraining { get; set; }
        public int Completed { get; set; }
        public int Inprogress { get; set; }
        public decimal CompletionPercent { get; set; } = 0.0M;
    }
    public class TrainingStatics
    {
        public DateTime Month { get; set; }
        public Training Training { get; set; }
        public int TotalTrainingAssign { get; set; }
        public int TrainingCompleted { get; set; }
        public int PhisedAfterTraining { get; set; }
        public int TrainingInprogess { get; set; }
        public int TrainingNotAttampt { get; set; }
        public decimal TrainingCompromised { get; set; }

        //public decimal TrainingCompromised
        //{
        //    get
        //    {
        //        if (TotalTrainingAssign == 0)
        //            return 0;
        //        var compromise = Math.Round(((decimal)TrainingCompleted / PhisedAfterTraining) * 100, 2);
        //        return compromise;
        //    }
        //}
    }

}
