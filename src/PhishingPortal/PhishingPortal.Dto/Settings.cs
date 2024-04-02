using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using PhishingPortal.Common;

namespace PhishingPortal.Dto
{
    public class AzureRegistrationSettings
    {
        [Required]
        public string ClientID { get; set; }
        [Required]
        public string ClientSecret { get; set; }
        [Required]
        public string TenantID { get; set; }

        public AzureRegistrationSettings()
        {
            ClientID = string.Empty;
            ClientSecret = string.Empty;
            TenantID = string.Empty;
        }

        public AzureRegistrationSettings(string clientId, string clientSecret, string tenantID)
        {
            ClientID = clientId;
            ClientSecret = clientSecret;
            TenantID = tenantID;
        }

        public AzureRegistrationSettings(Dictionary<string, string> values)
        {
            ClientID = values[Constants.Keys.ClIENT_ID] ?? String.Empty;
            ClientSecret = values[Constants.Keys.CLIENT_SECRET] ?? String.Empty;
            TenantID = values[Constants.Keys.TENANT_ID] ?? String.Empty;
        }

        public Dictionary<string, string> ToSettingsDictionary()
        {
            var result = new Dictionary<string, string>();

            if(ClientID != null)
                result.Add(Constants.Keys.ClIENT_ID, ClientID);

            if(ClientSecret != null)
                result[Constants.Keys.CLIENT_SECRET] = ClientSecret;

            if(TenantID != null)
                result[Constants.Keys.TENANT_ID] = TenantID;

            return result;

        }
    }

    public class OnPromiseADSettings
    {
        [Required]
        public string Domain { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Username { get; set; }

        public OnPromiseADSettings()
        {
            Domain = string.Empty;
            Password = string.Empty;
            Username = string.Empty;
        }

        public OnPromiseADSettings(string clientId, string clientSecret, string tenantID)
        {
            Domain = clientId;
            Password = clientSecret;
            Username = tenantID;
        }

        public OnPromiseADSettings(Dictionary<string, string> values)
        {
            Domain = values[Constants.Keys.OnPromiseAD_Doamin] ?? String.Empty;
            Password = values[Constants.Keys.OnPromiseAD_Password] ?? String.Empty;
            Username = values[Constants.Keys.OnPromiseAD_Username] ?? String.Empty;
        }

        public Dictionary<string, string> ToSettingsDictionary()
        {
            var result = new Dictionary<string, string>();

            if (Domain != null)
                result.Add(Constants.Keys.OnPromiseAD_Doamin, Domain);

            if (Password != null)
                result[Constants.Keys.OnPromiseAD_Password] = Password;

            if (Username != null)
                result[Constants.Keys.OnPromiseAD_Username] = Username;

            return result;

        }

       
    }

    public class WeeklySummaryReportSettings
    {
        public bool IsEnabled { get; set; }

        [Required]
        public string ReportRecipentsEmails { get; set; }

        public WeeklySummaryReportSettings()
        {
             IsEnabled = false;
             ReportRecipentsEmails = string.Empty;
        }

        public WeeklySummaryReportSettings(bool isEnabled, string reportRecipentsEmails)
        {
            IsEnabled = isEnabled;
            ReportRecipentsEmails = reportRecipentsEmails;
        }

        public WeeklySummaryReportSettings(Dictionary<string, string> values)
        {
            IsEnabled = bool.Parse(values.GetValueOrDefault(Constants.Keys.WeeklyReport_IsEnabled, "false"));
            ReportRecipentsEmails = values.GetValueOrDefault(Constants.Keys.WeeklyReport_RecipientsMail, String.Empty);
        }

        public Dictionary<string, string> ToSettingsDictionary()
        {
            var result = new Dictionary<string, string>();

            result[Constants.Keys.WeeklyReport_IsEnabled] = IsEnabled.ToString();
            result[Constants.Keys.WeeklyReport_RecipientsMail] = ReportRecipentsEmails;

            return result;
        }

    }

    public class ProviderSettings
    {
        //Email Config
        public string SmtpServer {  get; set; }
        public int Port { get; set; }

        //Sms Config
        public string SmsBaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderId { get; set; }
        public string EntityId { get; set; }
        public int MaxContentLength { get; set; }
        public string SmsSendUri { get; set; }
        public string SmsDeliveryUri { get; set; }

        //Whatsapp Config
        public string WhatsappBaseUri {  get; set; }
        public string WhatsappUri { get; set; }
        public string InstanceId { get; set; }

        public string AccessToken {  get; set; }

        //Training Config
        public bool TrainingReminder { get; set; }
        public string TrainingMailId {  get; set; }

        public ProviderSettings()
        {
            // Initialize properties with default values if needed
        }

        // Constructor to initialize all properties
        public ProviderSettings(
            string smtpServer, int port,
            string smsBaseUrl, string username, string password,
            string senderId, string entityId, int maxContentLength,
            string smsSendUri, string smsDeliveryUri,
            string whatsappBaseUri, string whatsappUri, string instanceId,
            string accessToken, bool trainingReminder, string trainingMailId)
        {
            SmtpServer = smtpServer;
            Port = port;
            SmsBaseUrl = smsBaseUrl;
            Username = username;
            Password = password;
            SenderId = senderId;
            EntityId = entityId;
            MaxContentLength = maxContentLength;
            SmsSendUri = smsSendUri;
            SmsDeliveryUri = smsDeliveryUri;
            WhatsappBaseUri = whatsappBaseUri;
            WhatsappUri = whatsappUri;
            InstanceId = instanceId;
            AccessToken = accessToken;
            TrainingReminder = trainingReminder;
            TrainingMailId = trainingMailId;
        }

        // Constructor to initialize from dictionary
        public ProviderSettings(Dictionary<string, string> values)
        {
            // Initialize properties from dictionary values
            SmtpServer = values.GetValueOrDefault(Constants.Keys.SMTP_SERVER, "");
            Port = int.Parse(values.GetValueOrDefault(Constants.Keys.PORT, "0"));
            SmsBaseUrl = values.GetValueOrDefault(Constants.Keys.SMS_BASE_URL, "");
            Username = values.GetValueOrDefault(Constants.Keys.SMS_USERNAME, "");
            Password = values.GetValueOrDefault(Constants.Keys.SMS_PASSWORD, "");
            SenderId = values.GetValueOrDefault(Constants.Keys.SMS_SENDER_ID, "");
            EntityId = values.GetValueOrDefault(Constants.Keys.SMS_ENTITY_ID, "");
            MaxContentLength = int.Parse(values.GetValueOrDefault(Constants.Keys.SMS_MAX_CONTENT_LENGTH, "0"));
            SmsSendUri = values.GetValueOrDefault(Constants.Keys.SMS_SEND_URI, "");
            SmsDeliveryUri = values.GetValueOrDefault(Constants.Keys.SMS_DELIVERY_URI, "");
            WhatsappBaseUri = values.GetValueOrDefault(Constants.Keys.WHATSAPP_BASE_URI, "");
            WhatsappUri = values.GetValueOrDefault(Constants.Keys.WHATSAPP_URI, "");
            InstanceId = values.GetValueOrDefault(Constants.Keys.WHATSAPP_INSTANCE_ID, "");
            AccessToken = values.GetValueOrDefault(Constants.Keys.ACCESS_TOKEN, "");
            TrainingReminder = bool.Parse(values.GetValueOrDefault(Constants.Keys.TRAINING_REMINDER, "false"));
            TrainingMailId = values.GetValueOrDefault(Constants.Keys.TRAINING_MAIL_ID, "");
        }


        // Method to convert to dictionary
        public Dictionary<string, string> ToSettingsDictionary()
        {
            var result = new Dictionary<string, string>();
            result[Constants.Keys.SMTP_SERVER] = SmtpServer;
            result[Constants.Keys.PORT] = Port.ToString();
            result[Constants.Keys.SMS_BASE_URL] = SmsBaseUrl;
            result[Constants.Keys.SMS_USERNAME] = Username;
            result[Constants.Keys.SMS_PASSWORD] = Password;
            result[Constants.Keys.SMS_SENDER_ID] = SenderId;
            result[Constants.Keys.SMS_ENTITY_ID] = EntityId;
            result[Constants.Keys.SMS_MAX_CONTENT_LENGTH] = MaxContentLength.ToString();
            result[Constants.Keys.SMS_SEND_URI] = SmsSendUri;
            result[Constants.Keys.SMS_DELIVERY_URI] = SmsDeliveryUri;
            result[Constants.Keys.WHATSAPP_BASE_URI] = WhatsappBaseUri;
            result[Constants.Keys.WHATSAPP_URI] = WhatsappUri;
            result[Constants.Keys.WHATSAPP_INSTANCE_ID] = InstanceId;
            result[Constants.Keys.ACCESS_TOKEN] = AccessToken;
            result[Constants.Keys.TRAINING_REMINDER] = TrainingReminder.ToString();
            result[Constants.Keys.TRAINING_MAIL_ID] = TrainingMailId;

            return result;
        }

    }
}
