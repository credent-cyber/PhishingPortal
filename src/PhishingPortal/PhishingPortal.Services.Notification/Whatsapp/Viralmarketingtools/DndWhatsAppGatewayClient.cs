using HtmlAgilityPack;
using Microsoft.Graph.ExternalConnectors;
using PhishingPortal.Services.Notification.Whatsapp.Deal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PhishingPortal.Services.Notification.Whatsapp.Viralmarketingtools
{
    public class DndWhatsAppGatewayClient : IWhatsappGatewayClient
    {
        //private const string WhatsappUri = "api/send";
        private readonly HttpClient httpClient_;
        public DndWhatsAppGatewayClient(ILogger<DndWhatsAppGatewayClient> logger, DndlWhatsAppGatewayClientConfig config, IConfiguration configuration)
        {
            Logger = logger;
            Config = config;
            httpClient_ = new HttpClient();
            httpClient_.BaseAddress = new Uri(Config.BaseUrl);
            Configuration = configuration;
        }

        public ILogger<DndWhatsAppGatewayClient> Logger { get; }

        public DndlWhatsAppGatewayClientConfig Config { get; }
        IConfiguration Configuration { get; }


        public Task<bool> Send(string to, string from, string message)
        {
            return Send(to, from, message, string.Empty, string.Empty);
        }

        public async Task<bool> Send(string to, string from, string message, string mediaUrl, string file)
        {

            string BaseUrl = Configuration.GetSection("DndWhatsappGateway:BaseUrl").Value;
            string WhatsappUri = Configuration.GetSection("DndWhatsappGateway:WhatsappUri").Value;
            string InstanceId = Configuration.GetSection("DndWhatsappGateway:InstanceId").Value;
            string AccessToken = Configuration.GetSection("DndWhatsappGateway:AccessToken").Value;
            bool IsEnabled = bool.Parse(Configuration.GetSection("DndWhatsappGateway:IsEnabled").Value);

            Logger.LogInformation($"Sending whatsapp message to:[{to}]");
            string formatedMessage = GetWhatsAppFormattedText(message);
            //string formatedMessage = GetPlainTextFromHtml(message);

            if (!IsEnabled)
            {
                Logger.LogWarning($"Whatsapp sending is disabled at moment, please refer to the settings");

                return await Task.FromResult(true);
            }

            try
            {
                to = to.Length == 10 ? "91" + to : to;
                var uri = $"{BaseUrl}/{WhatsappUri}?number={to}&type=text&message={formatedMessage}";

                if (!string.IsNullOrEmpty(mediaUrl) && !string.IsNullOrEmpty(file))
                {
                    uri += $"&media_url={mediaUrl}&filename={file}";
                }

                uri += $"&instance_id={InstanceId}&access_token={AccessToken}";

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

        public string GetWhatsAppFormattedText(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Start with an empty string builder
            var whatsappFormattedText = new System.Text.StringBuilder();

            // Process the child nodes of the document root
            foreach (var node in doc.DocumentNode.ChildNodes)
            {
                whatsappFormattedText.Append(EncodeNodeForWhatsApp(node));
            }

            // Return the trimmed result to remove any leading or trailing spaces or new lines
            return whatsappFormattedText.ToString().Trim();
        }

        public string EncodeNodeForWhatsApp(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                return HttpUtility.UrlEncode(node.InnerText.Trim());
            }
            else if (node.Name.Equals("b", StringComparison.OrdinalIgnoreCase))
            {
                return "*" + ProcessChildNodes(node) + "*";
            }
            else if (node.Name.Equals("i", StringComparison.OrdinalIgnoreCase))
            {
                return "_" + ProcessChildNodes(node) + "_";
            }
            else if (node.Name.Equals("u", StringComparison.OrdinalIgnoreCase))
            {
                return "__" + ProcessChildNodes(node) + "__";
            }
            else if (node.Name.Equals("s", StringComparison.OrdinalIgnoreCase))
            {
                return "~" + ProcessChildNodes(node) + "~";
            }
            else if (node.Name.Equals("ul", StringComparison.OrdinalIgnoreCase))
            {
                return String.Join("%0A", node.ChildNodes
                    .Where(child => child.Name.Equals("li", StringComparison.OrdinalIgnoreCase))
                    .Select(liNode => "• " + EncodeNodeForWhatsApp(liNode)));
            }
            else if (node.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
            {
                return "%0A" + ProcessChildNodes(node) + "%0A";
            }
            else if (node.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                // Handle hyperlinks, customize as needed
                string href = node.GetAttributeValue("href", "");
                return $"[{ProcessChildNodes(node)}]({HttpUtility.UrlEncode(href)})";
            }
            else if (node.Name.Equals("br", StringComparison.OrdinalIgnoreCase))
            {
                return "%0A"; // New line
            }

            return ProcessChildNodes(node);
        }

        private string ProcessChildNodes(HtmlNode node)
        {
            // Join child nodes with a space to ensure proper spacing between words
            return String.Join(" ", node.ChildNodes.Select(child => EncodeNodeForWhatsApp(child)));
        }


    }
}
