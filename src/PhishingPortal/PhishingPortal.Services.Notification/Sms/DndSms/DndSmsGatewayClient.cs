using Microsoft.Graph;
using PhishingPortal.Services.Notification.Sms.Deal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.Sms.DndSms
{
    public class DndSmsGatewayClient : ISmsGatewayClient
    {
        private const string SmsSendUri = "/sendsms.jsp?";
        private const string SmsDeliveryUri = "/getDLR.jsp?";
        public ILogger<DndSmsGatewayClient> Logger { get; }
        public DndSmsGatewayConfig Config { get; }
        public DndSmsGatewayClient(ILogger<DndSmsGatewayClient> logger, DndSmsGatewayConfig config)
        {
            Logger = logger;
            Config = config;
        }
       
        public async Task<bool> Send(string to, string from, string message)
        {
            if (!Config.IsEnabled)
            {
                Logger.LogWarning($"Sms sending is not enabled at this moment, refer to appsettings.json");
                return false;
            }
            StringBuilder sbPostData = new StringBuilder();

            sbPostData.AppendFormat("user={0}", Config.Username);

            sbPostData.AppendFormat("&password={0}", Config.Password);

            sbPostData.AppendFormat("&mobiles={0}", to);

            sbPostData.AppendFormat("&sms={0}", message);

            sbPostData.AppendFormat("&senderid={0}", Config.SenderId);

            sbPostData.AppendFormat("&accusage={0}", "1");
            sbPostData.AppendFormat("&tempid={0}", Config.tempid);
            sbPostData.AppendFormat("&entityid={0}", Config.entityid);
            sbPostData.AppendFormat("&shorturl={0}", "1");

            try
            {
                if (string.IsNullOrEmpty(message))
                    throw new ArgumentNullException("The sms message content cannot be empty");

                if (message.Length > Config.MaxContentLength)
                    throw new InvalidOperationException("Sms content greater than 160 characters cannot be sent");

                if (string.IsNullOrEmpty(from))
                {
                    from = Config.SenderId;
                }

                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(Config.BaseUrl+SmsSendUri);

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


                //Close the response

                reader.Close();
                response.Close();
                return responceStatus;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return false;
            }

        }

        public async Task<decimal> GetBalance()
        {
            throw new NotImplementedException();
        }
    }
}
