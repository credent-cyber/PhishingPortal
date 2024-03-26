using Microsoft.Extensions.Configuration;
using System.Security.Principal;

namespace PhishingPortal.Services.Utilities
{
    public class ApplicationSettings
    {
        public bool EnableWeeklyReportService { get; set; } = true;

        public int WaitIntervalInMinutes { get; set; }
        public ApplicationSettings(IConfiguration config)
        {
            config.GetSection("WorkerSettings").Bind(this);
        }
    }
}