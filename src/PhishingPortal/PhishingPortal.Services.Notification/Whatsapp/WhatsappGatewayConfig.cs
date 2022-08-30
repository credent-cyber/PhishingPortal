namespace PhishingPortal.Services.Notification.Whatsapp
{
    internal class WhatsappGatewayConfig
    {
        //private static string INSTANCE_ID = "YOUR_INSTANCE_ID_HERE";
        //private static string CLIENT_ID = "YOUR_CLIENT_ID_HERE";
        //private static string CLIENT_SECRET = "YOUR_CLIENT_SECRET_HERE";
        //private static string API_URL = "http://api.whatsmate.net/v3/whatsapp/single/text/message/" + INSTANCE_ID;

        public string InstanceId { get; set; } = "YOUR_INSTANCE_ID_HERE";
        public string ClientId { get; set; } = "YOUR_CLIENT_ID_HERE";
        public string ClientSecret { get; set; } = "YOUR_CLIENT_SECRET_HERE";
        public string ApiBaseUri { get; set; } = "http://api.whatsmate.net/v3/whatsapp/single/text/message/";
        public bool IsEnabled { get; set; }

        public WhatsappGatewayConfig(ILogger<WhatsappGatewayConfig> logger, IConfiguration config)
        {
            Logger = logger;
            Config = config;

            Config.GetSection("WhatsappMateGateway").Bind(this);

            Logger.LogInformation($"InstanceId: {InstanceId}");
            Logger.LogInformation($"ClientId: {ClientId}");
        }

        public ILogger<WhatsappGatewayConfig> Logger { get; }
        public IConfiguration Config { get; }
    }
}