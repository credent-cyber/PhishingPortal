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
    using PhishingPortal.Services.Notification;
    using System.Net;
    using System.Net.Mail;

    public class EmailPhishingReportMonitor : IPhishingEventReportWatcher, IPhishingEventReportProcessor
    {

        public string Source { get; } = Constants.Keys.SRC_MSGRAPH; // AZURE, EXCHANGE
        public string[] Scopes { get; } = new[] { "https://graph.microsoft.com/.default" };

        #region Exchange Mail box settings
       
        private const int MESSAGE_BATCH_SIZE = 10;
        private const string LAST_FETCH_CATCH_FILE_PATH = "lstfetchcatch.cache";
        private ExchangeVersion _exchangeVersion;
        private string _account;
        private string _pwd;
        private string _domain;
        private string _exchangeDiscoveryUrl;
        private string _correlationHeaderKey;
        private string _mailTrackerBlock = "####";
        #endregion

        #region Graphclient settings
        private string _clientID;
        private string _clientSecret;
        private string _tenantID;
        private string _emailAccount;
        private string _filters;
        private DateTime? _lastRunUtcDateTime;
        private DateTime? _lastAccessMailTimeStamp;
        private bool _isProcessing;
        #endregion

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly Tenant _tenant;
        private GraphServiceClient _graphClient = null;
        private readonly TenantDbContext _dbContext;
        private readonly List<TenantSetting> _settings;
        private readonly MailTrackerConfig _mailTrackerConfig;


        public EmailPhishingReportMonitor(ILogger logger, IConfiguration config, Tenant tenant, ITenantDbConnManager tenantDbConnManager)
        {
            _logger = logger;
            _config = config;
            _tenant = tenant;

            // set configurations
            _dbContext = tenantDbConnManager.GetContext(_tenant.UniqueId);
            _settings = _dbContext.Settings.AsNoTracking().ToList();

            _mailTrackerConfig = new MailTrackerConfig(config);
            _correlationHeaderKey =_mailTrackerConfig.CorrelationHeaderKey;
            _mailTrackerBlock = _mailTrackerConfig.MailTrackerBlock;

            Source = GetSetting(Constants.Keys.PHISHING_REPORT_REPOSITORY);
            if (string.IsNullOrEmpty(Source))
            {
                Source = Constants.Keys.SRC_MSGRAPH;
            }


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
                    if (string.IsNullOrEmpty(_emailAccount))
                        _emailAccount = "malay.pandey@credentinfotech.com";

                    return await ReadAzureMailBox(_emailAccount);

                case Constants.Keys.SRC_EXCHANGE:
                    throw new NotImplementedException("Reading Exchange Server message are not implemented");
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
            if (_isProcessing) return 0;

            _isProcessing = true;
            // persis the last run datetime stamp in a file and retrieve from there.
            if (!_lastRunUtcDateTime.HasValue)
                _lastRunUtcDateTime = GetLastActivityTimeStamp();

            var correlationValues = await CheckAsync();

            int count = 0;
            if (correlationValues == null)
                return count;

            foreach (var key in correlationValues)
            {
                count++;
                var log = await _dbContext.CampaignLogs.FirstOrDefaultAsync(o => o.SecurityStamp == key || o.ReturnUrl == key);
                if (log != null)
                {
                    log.IsReported = true;
                    log.ReportedBy = nameof(EmailPhishingReportMonitor);
                    log.ReportedOn = DateTime.UtcNow;

                    _dbContext.Update(log);
                }
            }

            if (_dbContext.ChangeTracker.HasChanges())
                await _dbContext.SaveChangesAsync();

            /// last accessed mail timestamp so that we start from that later.
            SetLastActivityTimestamp(_lastAccessMailTimeStamp);
            _isProcessing = false;
            return count;
        }

        private string GetSetting(string key)
        {

            var result = _settings.FirstOrDefault(s => s.Key == key);

            if (result == null)
            {
                return string.Empty;
            }

            return result.Value;
        }

        private async Task<List<string>> ReadAzureMailBox(string emailAccount)
        {

            var createDateTimeString = (_lastRunUtcDateTime.HasValue ? _lastRunUtcDateTime.Value : DateTime.UtcNow).ToString("yyyy-MM-ddTHH:mm:ssZ");

            //createDateTimeString = $"{createDateTimeString}T00:00:00Z";

            _logger.LogInformation($"Will be filtering mail created on or after [{createDateTimeString}]");

            var req = _graphClient.Users[emailAccount].MailFolders.Inbox.Messages.Request();

            if (!string.IsNullOrEmpty(_filters))      
                req = req.Filter(_filters);
            else
            {
                req = req.Filter($"isRead ne true AND ((createdDateTime ge {createDateTimeString}))");
            }

            _logger.LogInformation($"Reading mail box {emailAccount}");
            _logger.LogInformation($"Top:{MESSAGE_BATCH_SIZE} message will be processed in one go");
            
            var msgs = await req.Top(MESSAGE_BATCH_SIZE).GetAsync();   

            List<Message> messages = new();
            List<string> correlationIDs = new();

            messages.AddRange(msgs.CurrentPage);

            while (msgs.NextPageRequest != null)
            {
                await msgs.NextPageRequest.GetAsync();
                messages.AddRange(msgs.CurrentPage);
            }

            _logger.LogInformation($"Total message retrieved - [{msgs.Count()}]");
            // read all message and get correlation id from the header, if found extract and enlist if not discard the message 
            // by marking it as read
            foreach (var msg in messages)
            {
                // TODO: check if message is a reply to a phishing email 
                try
                {
                    var header = msg.InternetMessageHeaders?.FirstOrDefault(o => o.Name == _correlationHeaderKey);
                    if (header != null)
                    {
                        _logger.LogInformation($"Found customer header in the mail message -  [{_correlationHeaderKey}:{header.Value}]");
                        correlationIDs.Add(header.Value);
                    }

                    _logger.LogInformation($"Finding message key");
                    var startIndex = msg.Body.Content.IndexOf(_mailTrackerBlock) + 4;
                    var endIndex = msg.Body.Content.IndexOf(_mailTrackerBlock, startIndex);

                    if (endIndex < startIndex) continue;

                    var id = msg.Body.Content.Substring(startIndex, endIndex - startIndex);
                 
                    if (Guid.TryParse(id, out Guid guid))
                    {
                        _logger.LogInformation($"Found message key - {id}");
                        correlationIDs.Add(guid.ToString());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                }

            }

            if (messages?.Count() > 0)
            {
                var maxCreateDt = messages?.Max(o => o.CreatedDateTime.Value);
                _lastAccessMailTimeStamp = maxCreateDt?.DateTime;
            }

            _logger.LogInformation($"Following message will be processed:- ", correlationIDs);

            return correlationIDs;
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

            service.Credentials = new NetworkCredential(_account, _pwd, _domain);

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

        private DateTime GetLastActivityTimeStamp()
        {
            var dt = DateTime.UtcNow;

            if (System.IO.File.Exists(GetFileName()))
            {
                var content = System.IO.File.ReadAllText(GetFileName());
                if(!DateTime.TryParse(content, out dt))
                {
                    _logger.LogInformation($"Last activity timestamp couldn't be retrieved, defaulting to UtcNow: [{dt.ToString()}]");
                }
            }
            else
            {
                SetLastActivityTimestamp(dt);
            }

            return dt;
        }

        private void SetLastActivityTimestamp(DateTime? dt)
        {
            if(dt.HasValue)
                System.IO.File.WriteAllText(GetFileName(), dt.ToString());
        }

        private string GetFileName()
        {
            return $"{_tenant.UniqueId}-{LAST_FETCH_CATCH_FILE_PATH}";
        }
    }
}