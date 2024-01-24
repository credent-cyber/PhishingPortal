using Microsoft.Graph;

namespace PhishingPortal.Services.Notification.Whatsapp.Deal
{
    internal class DealWhatsAppGatewayClient //: IWhatsappGatewayClient
    {
        private readonly HttpClient httpClient_;
        public DealWhatsAppGatewayClient(ILogger<DealWhatsAppGatewayClient> logger, DealWhatsAppGatewayClientConfig config)
        {
            Logger = logger;
            Config = config;
            httpClient_ = new HttpClient();
            httpClient_.BaseAddress = new Uri(Config.BaseUrl);
        }

        public ILogger<DealWhatsAppGatewayClient> Logger { get; }

        public DealWhatsAppGatewayClientConfig Config { get; }

        public Task<bool> Send(string to, string from, string message)
        {
            return Send(to, from, message, string.Empty, string.Empty);
        }

        public async Task<bool> Send(string to, string from, string message, string mediaUrl, string file)
        {
            Logger.LogInformation($"Sending whatsapp message to:[{to}]");

            if (!Config.IsEnabled)
            {
                Logger.LogWarning($"Whatsapp sending is disabled at moment, please refer to the settings");

                return await Task.FromResult(true);
            }

            try
            {
                var uri = $"{Config.BaseUrl}/api/send.php?number={to}&type=text&message={message}";

                if (!string.IsNullOrEmpty(mediaUrl) && !string.IsNullOrEmpty(file))
                {
                    uri += $"&media_url={mediaUrl}&filename={file}";
                }

                uri += $"&instance_id={Config.InstanceId}&access_token={Config.AccessToken}";

                Logger.LogDebug(uri);

                var response = await httpClient_.GetAsync(uri);

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                    Logger.LogInformation($"Whatsapp message sent");
               
                var content = await response.Content.ReadAsStringAsync();
                
                Logger.LogInformation($"WhatsApi Response Message: {content}");
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"There was an error while sending Whatsapp message");
            }

            return await Task.FromResult(false);
        }
    }
}