namespace PhishingPortal.Services.Notification.Whatsapp.Deal
{
    internal class DealWhatsAppGatewayClientConfig
    {
        public string BaseUrl { get; set; } = "https://dealsms.in";
        public string InstanceId { get; set; } = "641864A8AA901";
        public string AccessToken { get; set; } = "6511cc6939dc30cff0874eac82c8121b";
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