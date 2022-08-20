using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhishingPortal.Dto
{
    public class AzureRegistrationSettings
    {

        public const string ClIENT_ID = "az_client_id";
        public const string CLIENT_SECRET = "az_client_secret";
        public const string TENANT_ID = "az_tenant_id";

        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
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
            ClientID = values[ClIENT_ID] ?? String.Empty;
            ClientSecret = values[CLIENT_SECRET] ?? String.Empty;
            TenantID = values[TENANT_ID] ?? String.Empty;
        }

        public Dictionary<string, string> ToSettingsDictionary()
        {
            var result = new Dictionary<string, string>();

            if(ClientID != null)
                result.Add(ClIENT_ID, ClientID);

            if(ClientSecret != null)
                result[CLIENT_SECRET] = ClientSecret;

            if(TenantID != null)
                result[TENANT_ID] = TenantID;

            return result;

        }
    }
}
