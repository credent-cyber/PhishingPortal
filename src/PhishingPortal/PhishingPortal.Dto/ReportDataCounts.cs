using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class ReportDataCounts
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; }
        public int Total { get; set; }
        public int Hits { get; set; }
        public int Reported { get; set; }
    }

    public class DrillDownReportCountParameter
    {
        public string Ids { get; set; }
        public string type { get; set; }
        public string filter { get; set; }
    }
}
