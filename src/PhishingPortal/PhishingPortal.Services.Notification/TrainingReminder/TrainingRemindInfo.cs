using PhishingPortal.Services.Notification.WeeklySummaryReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.TrainingReminder
{
    public class TrainingRemindInfo
    {
        public string TenantIdentifier { get; set; }
        public EmailDetails EmailDetails { get; set; }

    }
    public class EmailDetails
    {
        public string Recipient { get; set; }
        public string Subject { get; set; } = "Reminder: Complete Your Phishing Simulation Training";
        public string Content { get; set; }
        public string DisplayName { get; set; } = "PhishSims";
        public string From { get; set; } = "reminder@phishsims.com";
    }
}
