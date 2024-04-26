namespace PhishingPortal.Services.Notification.Sms.Deal
{
    public class DealSmsGatewayConfig
    {
        public string BaseUrl { get; set; } = "http://bhashsms.com/";
        public string Username { get; set; } = "success";
        public string Password { get; set; } = "sms@2023";
        public string Sender { get; set; } = "BHAINF";
        public string Priority { get; set; } = "ndnd";
        public string Type { get; set; } = "normal";
        public bool IsEnabled { get; set; } = true;
        public int MaxContentLength { get; set; } = 150;

        public DealSmsGatewayConfig(ILogger<DealSmsGatewayConfig> logger, IConfiguration appsettings)
        {
            appsettings.GetSection("DealSmsGateway").Bind(this);

            logger.LogInformation($"{nameof(BaseUrl)}: {BaseUrl}");
            logger.LogInformation($"{nameof(Username)}: {Username}");
            logger.LogInformation($"{nameof(Password)}: *************");
            logger.LogInformation($"{nameof(Sender)}: {Sender}");
            logger.LogInformation($"{nameof(Priority)}: {Priority}");
            logger.LogInformation($"{nameof(Type)}: {Type}");
            logger.LogInformation($"{nameof(IsEnabled)}: {IsEnabled}");
            logger.LogInformation($"{nameof(MaxContentLength)}: {MaxContentLength}");
        }
    }
}