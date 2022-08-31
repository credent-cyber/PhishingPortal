namespace PhishingPortal.Services.Notification.Sms
{
    public class AmyntraSmsGatewayConfig
    {
        public const string URI_SEND_SMS = "/sendsms/sendsms.php";
        public const string URI_GET_BAL = "/sendsms/checkbalance.php";
        public string BaseUrl { get; set; }
        public string Username { get; set;  }
        public string Password { get; set; }
        public string Sender { get; set; }
        public int MaxContentLength { get; set; }
        public bool IsEnabled { get; set; }

        public AmyntraSmsGatewayConfig(ILogger<AmyntraSmsGatewayConfig> logger, IConfiguration config)
        {
            config.GetSection("SmsGatewayClient").Bind(this);
        }
    }
}