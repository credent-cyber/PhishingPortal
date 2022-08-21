using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhishingPortal.Common;

namespace PhishingPortal.Dto
{
    public class AzureRegistrationSettings
    {

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
}
