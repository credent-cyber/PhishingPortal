using PhishingPortal.Core;
using Microsoft.Graph;
using Microsoft.Exchange.WebServices.Data;
using PhishingPortal.Dto;
using PhishingPortal.DataContext;
using Azure.Identity;

namespace PhishingPortal.Services.Notification.Monitoring
{
    using Microsoft.EntityFrameworkCore;
    using PhishingPortal.Common;
    using System.Net;

    public class EmailPhishingReportMonitor : IPhishingEventReportWatcher, IPhishingEventReportProcessor
    {
       
        public string Source { get; } = Constants.Keys.SRC_MSGRAPH; // AZURE, EXCHANGE
        public string[] Scopes { get; } = new[] { "https://graph.microsoft.com/.default" };

        #region Exchange Mail box settings
        private ExchangeVersion _exchangeVersion;
        private string _account;
        private string _pwd;
        private string _domain;
        private string _exchangeDiscoveryUrl;
        #endregion

        #region Graphclient settings
        private string _clientID;
        private string _clientSecret;
        private string _tenantID;
        private string _emailAccount;
        private string _filters;
        #endregion

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly Tenant _tenant;
        private GraphServiceClient _graphClient;
        private readonly TenantDbContext _dbContext;
        private readonly List<TenantSetting> _settings;

        public EmailPhishingReportMonitor(ILogger logger, IConfiguration config, Tenant tenant, ITenantDbConnManager tenantDbConnManager)
        {
            _logger = logger;
            _config = config;
            _tenant = tenant;

            // set configurations
            _dbContext = tenantDbConnManager.GetContext(_tenant.UniqueId);
            _settings = _dbContext.Settings.AsNoTracking().ToList();

            Source = GetSetting(Constants.Keys.PHISHING_REPORT_REPOSITORY);

            #region Fetch configuration from the database

            _clientID = GetSetting(Constants.Keys.ClIENT_ID);
            _clientSecret = GetSetting(Constants.Keys.CLIENT_SECRET);
            _tenantID = GetSetting(Constants.Keys.TENANT_ID);
            _emailAccount = GetSetting(Constants.Keys.MonitoredMailBoxAccount);
            _filters = GetSetting(Constants.Keys.MonitoredMailBoxFilters);


            Enum.TryParse(GetSetting(Constants.Keys.EXCHANGE_VERSION), out ExchangeVersion exVersion);
            _exchangeVersion = exVersion;

            _account = GetSetting(Constants.Keys.EXCHANGE_ACCOUNT);
            _pwd = GetSetting(Constants.Keys.EXCHANGE_ACCOUNT_PASSWORD);
            _domain = GetSetting(Constants.Keys.EXCHANGE_ACCOUNT_DOMAIN);
            _exchangeDiscoveryUrl = GetSetting(Constants.Keys.EXCHANGE_DISCOVERY_URL);


            if (Source == Constants.Keys.SRC_MSGRAPH)
            {
                var options = new TokenCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };

                var clientSecretCredential = new ClientSecretCredential(_tenantID, _clientID, _clientSecret, options);

                _graphClient = new GraphServiceClient(clientSecretCredential, Scopes);
            } 
            #endregion

        }

        /// <summary>
        /// CheckAsync
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> CheckAsync()
        {
            switch (Source)
            {
                case Constants.Keys.SRC_MSGRAPH:
                    return await ReadAzureMailBox(_emailAccount);

                case Constants.Keys.SRC_EXCHANGE:
                    return ReadExchangeMailBox();
            }

            return Enumerable.Empty<string>().ToList();
        }

        /// <summary>
        /// ProcessAsync
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public async Task<int> ProcessAsync()
        {
            var events = await CheckAsync();

            int count = 0;
            if (events == null)
                return count;

            foreach(var evt in events)
            {
                count++;
                var log = await _dbContext.CampaignLogs.FirstOrDefaultAsync(o => o.ReturnUrl == evt);
                if(log != null)
                {
                    log.IsReported = true;
                    log.ReportedBy = "monitoring";
                    log.ReportedOn = DateTime.UtcNow;

                    _dbContext.Update(log);
                }
            }

            _dbContext.ChangeTracker.HasChanges();
            await _dbContext.SaveChangesAsync();

            return count;
        }

        private string GetSetting(string key){
          
            var result = _settings.FirstOrDefault(s => s.Key == key);
            
            if(result == null)
            {
                return string.Empty;
            }

            return result.Value;
        }

        private async Task<List<string>> ReadAzureMailBox(string emailAccount)
        {
            var req = _graphClient.Users[emailAccount].Messages.Request();
            
            if (!string.IsNullOrEmpty(_filters))
                req = req.Filter(_filters);

            IUserMessagesCollectionPage msgs = await req.GetAsync();
            
            List<Message> messages = new();
            List<string> urls = new();
            
            messages.AddRange(msgs.CurrentPage);
            
            while (msgs.NextPageRequest != null)
            {
                await msgs.NextPageRequest.GetAsync();
                messages.AddRange(msgs.CurrentPage);
            }
            foreach(var msg in messages)
            {
                var subject = msg.Subject;
                urls.Add(subject);
            }
            return urls;
        }

        /// <summary>
        /// EWS services
        /// </summary>
        /// <param name="emailAccount"></param>
        /// <returns></returns>
        private List<string> ReadExchangeMailBox()
        {
            var result = new List<string>();
            ExchangeService service = new ExchangeService(_exchangeVersion);

            service.Credentials = new NetworkCredential( _account, _pwd, _domain );

            service.AutodiscoverUrl(_exchangeDiscoveryUrl);

            FindItemsResults<Item> findResults = service.FindItems(
               WellKnownFolderName.Inbox,
               new ItemView(10)
            );

            foreach (Item item in findResults.Items)
            {
                result.Add(item.Subject);
            }

            return result;
        }
    }
}