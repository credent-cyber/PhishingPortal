using PhishingPortal.Services.Notification.Sms.Deal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.Sms.DndSms
{
    public class DndSmsGatewayConfig
    {
        public string BaseUrl { get; set; } = "http://dndsms.reliableindya.info";
        public string Username { get; set; } = "demo08";
        public string Password { get; set; } = "6008b6a7edXX";
        public string SenderId { get; set; } = "RELABL";
        public string tempid { get; set; } = "1707163211312635261";
        public string entityid { get; set; } = "1401529690000019209";
        public bool IsEnabled { get; set; } = true;
        public int MaxContentLength { get; set; } = 150;

        public DndSmsGatewayConfig(ILogger<DndSmsGatewayConfig> logger, IConfiguration appsettings)
        {
            appsettings.GetSection("DealSmsGateway").Bind(this);

            logger.LogInformation($"{nameof(BaseUrl)}: {BaseUrl}");
            logger.LogInformation($"{nameof(Username)}: {Username}");
            logger.LogInformation($"{nameof(Password)}: *************");
            logger.LogInformation($"{nameof(SenderId)}: {SenderId}");
            logger.LogInformation($"{nameof(tempid)}: {tempid}");
            logger.LogInformation($"{nameof(entityid)}: {entityid}");
            logger.LogInformation($"{nameof(IsEnabled)}: {IsEnabled}");
            logger.LogInformation($"{nameof(MaxContentLength)}: {MaxContentLength}");
        }
    }
}
