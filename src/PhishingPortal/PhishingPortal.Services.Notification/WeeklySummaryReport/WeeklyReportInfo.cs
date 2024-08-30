using System;

namespace PhishingPortal.Services.Notification.WeeklySummaryReport
{
    public class WeeklyReportInfo
    {
        // Tenant identifier
        public string TenantIdentifier { get; set; }

        // Email details
        public EmailDetails EmailDetails { get; set; }

        // Campaign statistics
        public CampaignStatistics CampaignStatistics { get; set; }

        // Training statistics
        public TrainingStatistics TrainingStatistics { get; set; }
    }

    // Class for email details
    public class EmailDetails
    {
        public string Recipients { get; set; }
        public string Subject { get; set; } = "Weekly Report Of Your Phishing Simulation";
        public string Content { get; set; }
        public string From { get; set; } = "info@phishsims.com";
    }

    // Class for campaign statistics
    public class CampaignStatistics
    {
        // Email campaign statistics
        public CampaignTypeStatistics EmailStatistics { get; set; }

        // SMS campaign statistics
        public CampaignTypeStatistics SmsStatistics { get; set; }

        // WhatsApp campaign statistics
        public CampaignTypeStatistics WhatsappStatistics { get; set; }

        public DepartmentHitReport departmentHitReport { get; set; }
    }

    // Class for training statistics
    public class TrainingStatistics
    {
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Incomplete { get; set; }
        public decimal CompletionPercentage { get; set; }

        public List<DepartmentWiseTrainingStatistics> departmentWiseTrainingStatistics { get; set; }
    }

    // Class for campaign type statistics
    public class CampaignTypeStatistics
    {
        public int TotalCampaigns { get; set; }
        public int TotalHits { get; set; }
        public int TotalReported { get; set; }
        public decimal PronePercentage { get; set; }
        public string MostPhishingDepartment { get; set; }
    }

    // Class for department completion information
    public class DepartmentWiseTrainingStatistics
    {
        public string DepartmentName { get; set; }
        public int Assigned { get; set; }
        public int Completed { get; set; }
        public int InCompleted { get; set; }
        public int NotAttempt { get; set; }
        public decimal CompletionPercentage { get; set; }
    }
    public class DepartmentHitReport
    {
        public List<DepartmentReport> EmailCampaignDepartmentReport { get; set; }
        public List<DepartmentReport> SmsCampaignDepartmentReport { get; set; }
        public List<DepartmentReport> WhatsappCampaignDepartmentReport { get; set; }
    }

    public class DepartmentReport
    {
        public string DepartmentName { get; set; }
        public int TotalSent { get; set; }
        public int TotalHits { get; set; }
        public int TotalReported { get; set; }
        public decimal PronePercentage { get; set; }
        public string MostPhishingDepartment { get; set; }
    }
}
