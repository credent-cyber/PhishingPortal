namespace PhishingPortal.Services.Notification.Whatsapp.Deal
{
    internal class DealWhatsAppGatewayClientConfig
    {
        public string BaseUrl { get; set; } = "https://dealsms.in/api/";
        public string InstanceId { get; set; } = "63E5F247D7D9D";
        public string AccessToken { get; set; } = "783c698eef4306a2d370c9de3862744c";
        public bool IsEnabled { get; set; } = true;

        public DealWhatsAppGatewayClientConfig(ILogger<DealWhatsAppGatewayClientConfig> logger, IConfiguration appsettings)
        {
            appsettings.GetSection("DealWhatsAppGatewayClient").Bind(this);

            logger.LogInformation($"{nameof(BaseUrl)}: {BaseUrl}");
            logger.LogInformation($"{nameof(InstanceId)}: {InstanceId}");
            logger.LogInformation($"{nameof(IsEnabled)}: {IsEnabled}");

        }

    }
}