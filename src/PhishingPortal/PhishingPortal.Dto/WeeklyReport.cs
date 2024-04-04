using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class WeeklyReport : BaseEntity
    {
        public string Tenant {  get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool IsReportSent {  get; set; }
    }
}
