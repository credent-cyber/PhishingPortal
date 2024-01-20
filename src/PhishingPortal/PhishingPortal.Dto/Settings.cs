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

    public class OnPromiseADSettings
    {

        public string Domain { get; set; }
        public string Password { get; set; }
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
}
