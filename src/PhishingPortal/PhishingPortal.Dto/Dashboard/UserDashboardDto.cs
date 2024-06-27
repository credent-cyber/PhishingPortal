using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto.Dashboard
{

    public class UserDashboardDto
    {
        public int TotalTraining { get; set; }
        public int TrainingCompleted { get; set; }
        public int TrainingIncompleted { get; set; }

        public decimal ComplitionPercentage
        {
            get
            {
                if (TotalTraining == 0)
                    return 0;
                //var prone = (Hits / Count) * 100;
                var prone = Math.Round(((decimal)TrainingCompleted / TotalTraining) * 100, 2);
                return prone;
            }
        }
        public List<MonthlyTrainingReportData> MonthwiseTrainingEntry { get; set; }
    }

}
