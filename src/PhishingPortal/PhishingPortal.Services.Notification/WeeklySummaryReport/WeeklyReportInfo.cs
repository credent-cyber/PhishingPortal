using PhishingPortal.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.WeeklySummaryReport
{
    public class WeeklyReportInfo
    {
        public string Tenantdentifier { get; set; }
        public string EmailRecipients { get; set; }
        public string EmailSubject { get; set; }
        public string EmailContent { get; set; }
        public string EmailFrom { get; set; }
        public CampaignLog LogEntry { get; set; }

        public TrainingLog TrainingLog { get; set; }
    }
}
