using Azure.Identity;
using Microsoft.Graph;
using PhishingPortal.Repositories;
using PhishingPortal.Server.Services.Interfaces;

namespace PhishingPortal.Server.Services
{
    public class AzActDirClientService : IAzActDirClientService
    {

        private readonly GraphServiceClient _graphClient;

        public ILogger Logger { get; }
        public ISettingsRepository Settings { get; }
        public string[] Scopes { get; } = new[] { "https://graph.microsoft.com/.default" };
        public string ClientID { get; }
        public string ClientSecret { get; }
        public string TenantID { get; }

        public AzActDirClientService(ILogger logger, ISettingsRepository settings)
        {
            Logger = logger;
            Settings = settings;

            #region Init graph client object

            ClientID = Settings.GetSetting<string>("az_client_id").Result;
            ClientSecret = Settings.GetSetting<string>("az_client_secret").Result;
            TenantID = Settings.GetSetting<string>("az_tenant_id").Result ?? "cf92019c-152d-42f6-bbcc-0cf96e6b0108";

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };
            if (ClientID != null && ClientID != "")
            {
                var clientSecretCredential = new ClientSecretCredential(TenantID, ClientID, ClientSecret, options);

                _graphClient = new GraphServiceClient(clientSecretCredential, Scopes);
            }

            #endregion
        }

        public async Task<Dictionary<string, string>?> GetAllUserGroups()
        {
            var defaultValue = new Dictionary<string, string>();
            try
            {
                if (_graphClient is not null)
                {

                    var result = await _graphClient.Groups
                            .Request()
                            .GetAsync();

                    if (result == null)
                        return new Dictionary<string, string>();

                    return result.OrderBy(o => o.DisplayName).ToList().Where(o => o.Visibility?.ToLower() == "private")
                        .ToDictionary(k => k.Id, v => v.DisplayName);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return defaultValue;
            }
        }

        public async Task<List<User>> GetAdUsers()
        {
            var result = await _graphClient.Users
               .Request()
               .GetAsync();

            if (result == null)
                return Enumerable.Empty<User>().ToList();

            return result.CurrentPage.Where(o => !string.IsNullOrEmpty(o.EmployeeId)
                            && !string.IsNullOrEmpty(o.Mail)).ToList();

        }

        public async Task<List<User>> GetGroupMembers(string groupID)
        {
            var result = await _graphClient.Groups[groupID].Members.Request().GetAsync();

            if (result == null)
                return Enumerable.Empty<User>().ToList();

            var values = new List<User>();

            foreach (var member in result.CurrentPage)
            {
                var usr = member as Microsoft.Graph.User;

                if (usr != null)
                    values.Add(usr);
            }

            return values.ToList();

        }

    }


}
