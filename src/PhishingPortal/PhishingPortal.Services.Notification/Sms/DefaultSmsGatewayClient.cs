namespace PhishingPortal.Services.Notification.Sms
{
    public class DefaultSmsGatewayClient : ISmsGatewayClient
    {
        readonly HttpClient _httpClient;
        public bool IsEnabled { get; }
        public DefaultSmsGatewayClient(ILogger<DefaultSmsGatewayClient> logger, AmyntraSmsGatewayConfig config)
        {
            Logger = logger;
            Config = config;

            IsEnabled = config.IsEnabled;
            
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(config.BaseUrl);
        }

        public ILogger<DefaultSmsGatewayClient> Logger { get; }
        public AmyntraSmsGatewayConfig Config { get; }

        public virtual async Task<decimal> GetBalance()
        {
            var response = await _httpClient
                .GetAsync($"{AmyntraSmsGatewayConfig.URI_GET_BAL}?username={Config.Username}&password={Config.Password}&type=TEXT&sender={Config.Sender}");
            
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            if(decimal.TryParse(result, out decimal outcome))
            {
                return outcome;
            }
            return 0M;
        }

        public virtual async Task<bool> Send(string to, string from, string message)
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

                var response = await _httpClient
                    .GetAsync($"{AmyntraSmsGatewayConfig.URI_SEND_SMS}?username={Config.Username}&password={Config.Password}&type=TEXT&sender={Config.Sender}&mobile={to}&message={message}");

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