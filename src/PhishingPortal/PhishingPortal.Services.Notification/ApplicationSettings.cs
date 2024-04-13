using System.Security.Principal;

namespace PhishingPortal.Services.Notification
{
    public class ApplicationSettings
    {
        public bool EnableEmailCampaign { get; set; } = true;
        public bool EnableSmsCampaign { get; set; } = true;
        public bool EnableWhatsappCampaign { get; set; } = true;
        public bool EnableReportingMonitor { get; set; } = false;
        public bool EnableDemoRequestHandler { get; set; } = true;
        public bool EnableTrainingProvider { get; set; } = true;
        public bool EnableWeeklyReport { get; set; } = true;
        public int WaitIntervalInMinutes { get; set; }
        public ApplicationSettings(IConfiguration config)
        {
            config.GetSection("WorkerSettings").Bind(this);
        }
    }
}