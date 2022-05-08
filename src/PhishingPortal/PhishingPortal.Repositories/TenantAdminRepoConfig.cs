using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PhishingPortal.Repositories
{
    public class TenantAdminRepoConfig
    {
        public TenantAdminRepoConfig(ILogger<TenantAdminRepoConfig> logger, IConfiguration config)
        {
            config.GetSection("TenantAdminRepo").Bind(this);
        }

        public string TenantConfirmBaseUrl { get; set; } = "https://localhost:44307/tenant-confirmation/";
        public string CreatedBy { get; set; } = "System";
        public string DbNamePrefix { get; set; } = "Tenant-";
        public int DaysToConfirm { get; set; } = 10;
        public string ConnectionString { get; set; }

    }
} 