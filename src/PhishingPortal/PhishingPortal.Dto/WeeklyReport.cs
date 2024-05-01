using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class WeeklyReport : BaseEntity
    {
        public string ReportContent { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsReportSent { get; set; }
    }

}
