namespace PhishingPortal.Services.Notification.Sms.Deal
{
    public class DealSmsGatewayClient : ISmsGatewayClient
    {
        private const string SmsUri = "/api/sendmsg.php";
        private readonly HttpClient _httpClient;

        public DealSmsGatewayClient(ILogger<DealSmsGatewayClient> logger, DealSmsGatewayConfig config)
        {
            Logger = logger;
            Config = config;

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(config.BaseUrl);
        }

        public ILogger<DealSmsGatewayClient> Logger { get; }
        public DealSmsGatewayConfig Config { get; }

        public async Task<decimal> GetBalance()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Send(string to, string from, string message)
        {
            if (!Config.IsEnabled)
            {
                Logger.LogWarning($"Sms sending is not enabled at this moment, refer to appsettings.json");
                return false;
            }

            try
            {
                if (string.IsNullOrEmpty(message))
                    throw new ArgumentNullException("The sms message content cannot be empty");

                if (message.Length > Config.MaxContentLength)
                    throw new InvalidOperationException("Sms content greater than 160 characters cannot be sent");

                if (string.IsNullOrEmpty(from))
                {
                    from = Config.Sender;
                }

                var response = await _httpClient
                    .GetAsync($"{Config.BaseUrl}/{SmsUri}?user={Config.Username}&pass={Config.Password}&sender={from}&phone={to}&text={message}&priority={Config.Priority}&type={Config.Type}");

                var msg = response.EnsureSuccessStatusCode();

                return msg.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return false;
            }

        }
    }
}