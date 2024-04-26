using PhishingPortal.Services.Notification.Whatsapp.Deal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.Whatsapp.Viralmarketingtools
{
    public class DndlWhatsAppGatewayClientConfig
    {
        public string BaseUrl { get; set; } = "https://wa.viralmarketingtools.in";
        public string InstanceId { get; set; } = "65A7C8B36FE63";
        public string AccessToken { get; set; } = "6576c7c6da78d";
        public bool IsEnabled { get; set; } = true;

        public DndlWhatsAppGatewayClientConfig(ILogger<DndlWhatsAppGatewayClientConfig> logger, IConfiguration appsettings)
        {
            appsettings.GetSection("DndWhatsAppGatewayClient").Bind(this);

            logger.LogInformation($"{nameof(BaseUrl)}: {BaseUrl}");
            logger.LogInformation($"{nameof(InstanceId)}: {InstanceId}");
            logger.LogInformation($"{nameof(IsEnabled)}: {IsEnabled}");

        }
    }
}
