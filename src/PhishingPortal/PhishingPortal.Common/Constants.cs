namespace PhishingPortal.Common
{
    public class Constants
    {
        public class EmailTemplate
        {
            public const string TRAINING_CAMPAIGN = "Training";
        }

        public class Literals
        {
            public const string CREATED_BY = "system";
        }

        public class LocalStorage
        {
            public const string P_INDEX = "pIndex";
            public const string C_TAB = "cTab";
        }
        public class Keys
        {
            public const string PHISHING_REPORT_REPOSITORY = "mailbox_reporting_source";
            public const string SRC_MSGRAPH = "AZURE";
            public const string SRC_EXCHANGE = "EXCHANGE";

            public const string ClIENT_ID = "az_client_id";
            public const string CLIENT_SECRET = "az_client_secret";
            public const string TENANT_ID = "az_tenant_id";
            public const string EXCHANGE_VERSION = "exchange_version";

            public const string EXCHANGE_ACCOUNT = "exchange_account";
            public const string EXCHANGE_ACCOUNT_PASSWORD = "exchange_account_password";
            public const string EXCHANGE_ACCOUNT_DOMAIN = "exchange_account_domain";
            public const string EXCHANGE_DISCOVERY_URL = "exchange_discovery_url";
            public const string MonitoredMailBoxAccount = "monitored_mail_account";
            public const string RETURN_URL_COLLECTION = "return_urls";

            public const string OnPromiseAD_Doamin = "ad_domain";
            public const string OnPromiseAD_Username = "ad_username";
            public const string OnPromiseAD_Password = "ad_password";
    
            public static string MonitoredMailBoxFilters { get; set; }
        }
    }
}