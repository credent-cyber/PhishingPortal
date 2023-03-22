using System.Net;
using System.Threading;

namespace PhishingPortal.Services.Notification.Whatsapp
{
    using System.Net.Http.Json;

    internal class WhatsappMateGatewayClient : IWhatsappGatewayClient
    {
        public ILogger<WhatsappMateGatewayClient> Logger { get; }
        public WhatsappGatewayConfig GatewayConfig { get; }

        #region Payload Obj
        class Payload
        {
            public string number { get; set; }
            public string message { get; set; }
            public Payload(string number, string message)
            {
                this.number = number;
                this.message = message;
            }
        }

        #endregion
        public WhatsappMateGatewayClient(ILogger<WhatsappMateGatewayClient> logger, WhatsappGatewayConfig gatewayConfig)
        {
            Logger = logger;
            GatewayConfig = gatewayConfig;
        }
        
        public async Task<bool> Send(string to, string from, string message)
        {
            bool success = true;
            
            if(!GatewayConfig.IsEnabled)
            {
                Logger.LogWarning($"Whatsapp sending is disabled");
                return false;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(GatewayConfig.ApiBaseUri);
                    client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                    client.DefaultRequestHeaders.Add("X-WM-CLIENT-ID", GatewayConfig.ClientId);
                    client.DefaultRequestHeaders.Add("X-WM-CLIENT-SECRET", GatewayConfig.ClientSecret);

                    Payload payloadObj = new Payload(to, message);

                    Logger.LogInformation($"Sending whatsapp message to:[{to}]");
                    var response = await client
                        .PostAsJsonAsync<Payload>($"{GatewayConfig.ApiBaseUri}//v3/whatsapp/single/text/message/{GatewayConfig.InstanceId}", payloadObj);

                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode)
                        Logger.LogInformation($"Whatsapp message sent");
                    
                    return response.IsSuccessStatusCode;

                }
            }
            catch (Exception webEx)
            {
                Logger.LogCritical(webEx, webEx.Message);
                success = false;
            }

            return success;
        }

        public Task<bool> Send(string to, string from, string message, string mediaUrl, string file)
        {
            return Send(to, from, message);
        }
    }
}