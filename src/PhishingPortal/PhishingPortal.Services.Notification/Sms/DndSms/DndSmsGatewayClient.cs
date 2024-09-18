using Microsoft.Graph;
using Microsoft.Graph.ExternalConnectors;
using PhishingPortal.Services.Notification.Sms.Deal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PhishingPortal.Services.Notification.Sms.DndSms
{
    public class DndSmsGatewayClient : ISmsGatewayClient
    {
        //private const string SmsSendUri = "/sendsms.jsp?";
        //private const string SmsDeliveryUri = "/getDLR.jsp?";
        public ILogger<DndSmsGatewayClient> Logger { get; }
        public DndSmsGatewayConfig Config { get; }

        IConfiguration Configuration { get; }
        public DndSmsGatewayClient(ILogger<DndSmsGatewayClient> logger, DndSmsGatewayConfig config, IConfiguration configuration)
        {
            Logger = logger;
            Config = config;
            Configuration = configuration;
        }
       
        public async Task<(bool,string)> Send(string to, string from, string message, string TemplateId)
        {

            to = to.Length == 10 ? "+91" + to : to;
            bool IsEnabled = bool.Parse(Configuration.GetSection("DndSmsGateway:IsEnabled").Value);

            if (!IsEnabled)
            {
                Logger.LogWarning($"Sms sending is not enabled at this moment, refer to appsettings.json");
                return (false, null);
            }
            string BaseUrl = Configuration.GetSection("DndSmsGateway:BaseUrl").Value;
            string SmsSendUri = Configuration.GetSection("DndSmsGateway:SmsSendUri").Value;
            string SmsDeliveryUri = Configuration.GetSection("DndSmsGateway:SmsDeliveryUri").Value;
            int MaxContentLength = int.Parse(Configuration.GetSection("DndSmsGateway:MaxContentLength").Value);

            StringBuilder sbPostData = new StringBuilder();

            sbPostData.AppendFormat("user={0}", Configuration.GetSection("DndSmsGateway:Username").Value);

            sbPostData.AppendFormat("&password={0}", Configuration.GetSection("DndSmsGateway:Password").Value);

            sbPostData.AppendFormat("&mobiles={0}", to);

            sbPostData.AppendFormat("&sms={0}", message);

            sbPostData.AppendFormat("&senderid={0}", Configuration.GetSection("DndSmsGateway:SenderId").Value);

            sbPostData.AppendFormat("&accusage={0}", "1");
            sbPostData.AppendFormat("&tempid={0}", TemplateId.Trim());
            sbPostData.AppendFormat("&entityid={0}", Configuration.GetSection("DndSmsGateway:EntityId").Value);
            //sbPostData.AppendFormat("&shorturl={0}", "1");

            try
            {
                if (string.IsNullOrEmpty(message))
                    throw new ArgumentNullException("The sms message content cannot be empty");

                if (message.Length > MaxContentLength)
                    throw new InvalidOperationException("Sms content greater than 160 characters cannot be sent");

                if (string.IsNullOrEmpty(from))
                {
                    from = Configuration.GetSection("DndSmsGateway:SenderId").Value;
                }

                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(BaseUrl + SmsSendUri);

                //Prepare and Add URL Encoded data

                UTF8Encoding encoding = new UTF8Encoding();

                byte[] data = encoding.GetBytes(sbPostData.ToString());

                //Specify post method

                httpWReq.Method = "POST";

                httpWReq.ContentType = "application/x-www-form-urlencoded";

                httpWReq.ContentLength = data.Length;

                using (Stream stream = httpWReq.GetRequestStream())

                {
                    stream.Write(data, 0, data.Length);
                }

                //Get the response

                HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());

                string responseString = reader.ReadToEnd();

                bool responceStatus = response.StatusCode == HttpStatusCode.OK ? true : false;
                XDocument xmlDoc = XDocument.Parse(responseString);

                string messageidValue = xmlDoc.Descendants("messageid").FirstOrDefault()?.Value ?? null;
                

                //Close the response

                reader.Close();
                response.Close();
                return (responceStatus, messageidValue);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return (false, null);
            }

        }

        public async Task<decimal> GetBalance()
        {
            throw new NotImplementedException();
        }
    }
}
