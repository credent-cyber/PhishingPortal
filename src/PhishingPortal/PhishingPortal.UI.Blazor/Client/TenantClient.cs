using PhishingPortal.Dto;
using PhishingPortal.Dto.Dashboard;
using System.Net.Http.Json;

namespace PhishingPortal.UI.Blazor.Client
{
    public class TenantClient : BaseHttpClient
    {
        public TenantClient(ILogger<TenantClient> logger, HttpClient httpClient)
            : base(logger, httpClient)
        {

        }

        public async Task<IEnumerable<Campaign>> GetCampaignsAsync(int pageIndex, int pageSize)
        {
            IEnumerable<Campaign> campaigns;
            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/Campaigns/{pageIndex}/{pageSize}");

                res.EnsureSuccessStatusCode();

                campaigns = await res.Content.ReadFromJsonAsync<IEnumerable<Campaign>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return campaigns;
        }

        public async Task<Campaign> GetCampaingById(int id)
        {
            Campaign campaign = null;
            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/Campaign/{id}");

                res.EnsureSuccessStatusCode();

                campaign = await res.Content.ReadFromJsonAsync<Campaign>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return campaign;
        }

        public async Task<IEnumerable<CampaignTemplate>> GetCampaignTemplates()
        {
            List<CampaignTemplate> templates = Enumerable.Empty<CampaignTemplate>().ToList();

            try
            {
                var res = await HttpClient.GetAsync($"api/Tenant/CampaignTemplates");

                res.EnsureSuccessStatusCode();

                templates = await res.Content.ReadFromJsonAsync<List<CampaignTemplate>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return templates;
        }

        public async Task<Campaign> UpsertCampaignAsync(Campaign campaign)
        {

            Campaign result = null;

            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/Tenant/UpsertCampaign", campaign);

                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<Campaign>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;

        }

        /// <summary>
        /// Imports a list of recipients (uniquely identified by email id)
        /// </summary>
        /// <param name="recipients"></param>
        /// <returns></returns>
        public async Task<List<RecipientImport>> ImportRecipientToCampaign(int campaignId, List<RecipientImport> recipients)
        {
            ApiResponse<List<RecipientImport>> result;
            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/import-recipients-to-campaign/{campaignId}", recipients);

                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<ApiResponse<List<RecipientImport>>>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return await Task.FromResult(result.Result);
        }

        public async Task<List<CampaignRecipient>> GetRecipientByCampaignId(int campaignId)
        {
            List<CampaignRecipient> result = new List<CampaignRecipient>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/recipient-by-campaign/{campaignId}");

                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<List<CampaignRecipient>>();



            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return await Task.FromResult(result);
        }

        public async Task<List<CampaignTemplate>> GetTemplatesByType(CampaignType? type)
        {
            List<CampaignTemplate> result;

            try
            {
                var url = $"api/tenant/templates-by-type";
                if (type.HasValue)
                    url += $"/{type.Value}";
                var res = await HttpClient.GetAsync(url);
                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<List<CampaignTemplate>>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }

        public async Task<CampaignTemplate> GetTemplateById(int id)
        {
            CampaignTemplate result;

            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/template-by-id/{id}");
                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<CampaignTemplate>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }


        public async Task<CampaignTemplate> UpsertCampaignTemplate(CampaignTemplate template)
        {
            CampaignTemplate result;

            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/upsert-template", template);
                res.EnsureSuccessStatusCode();

                result = await res.Content.ReadFromJsonAsync<CampaignTemplate>();
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }

        public async Task<bool> CampaignHit(string urlKey)
        {
            try
            {
                var request = new GenericApiRequest<string>() { Param = urlKey };
                var res = await HttpClient.PostAsJsonAsync($"api/tenant/campaign-hit", request);
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();

                if (json != null)
                    return json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return false;
        }

        public async Task<MonthlyPhishingBarChart?> GetMonthlyBarChartEntires(int year)
        {
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/monthly-phishing-bar-chart-data/{year}");
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<MonthlyPhishingBarChart>>();

                if (json != null)
                    return json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return null;
        }

        public async Task<ConsolidatedPhishingStats> GetLatestStats()
        {
            // get-latest-statistics
            var result = new ConsolidatedPhishingStats();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/get-latest-statistics");
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<ConsolidatedPhishingStats>>();

                if (json != null)
                    result = json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return result;
        }
        public async Task<CategoryWisePhishingTestData?> GetCategoryWisePhishingTestData(DateTime startDate, DateTime endDate)
        {
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/category-wise-phising-test-prone-percent/{startDate.ToString("MM-dd-yyyy")}/{endDate.ToString("MM-dd-yyyy")}");
                res.EnsureSuccessStatusCode();


                var json = await res.Content.ReadFromJsonAsync<ApiResponse<CategoryWisePhishingTestData>>();

                if (json != null)
                    return json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return null;
        }

        #region Settings
        public async Task<Dictionary<string, string>> GetSettings()
        {
            var response = new Dictionary<string, string>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/settings");
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (json != null)
                    response = json;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }

            return response;
        }

        public async Task<Dictionary<string, string>> UpsertSettings(Dictionary<string, string> settings)
        {
            var response = new Dictionary<string, string>();
            try
            {
                var res = await HttpClient.PostAsJsonAsync<Dictionary<string, string>>($"api/tenant/upsert-settings", settings);
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (json != null)
                    response = json;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }

            return response;
        } 
        #endregion

        #region Related to Azure Active Directory Integration

        /// <summary>
        /// Get all Azure AD User Groups if available
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetAzureADUserGroups()
        {
            var response = new Dictionary<string, string>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/az-ad-groups");
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (json != null)
                    response = json;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }

            return response;
        }

        /// <summary>
        /// Get list of recipient groups Azure AD + non Ad groups
        /// </summary>
        /// <returns></returns>
        public async Task<List<RecipientGroup>> GetRecipientUserGroups()
        {
            var response = new List<RecipientGroup>();
            try
            {
                var res = await HttpClient.GetAsync($"api/tenant/recipient-user-groups");
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<List<RecipientGroup>>>();
                if (json != null)
                    response = json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }

            return response;
        }

        /// <summary>
        /// Mark Azure AD User groups to be imported
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public async Task<List<Recipient>> ImportAzureADByUserGroups(RecipientGroup grp)
        {
            var response = new List<Recipient>();

            try
            {
                var res = await HttpClient.PostAsJsonAsync<RecipientGroup>($"api/tenant/az-ad-user-groups-import", grp);
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadFromJsonAsync<ApiResponse<List<Recipient>>>();
                if (json != null)
                    response = json.Result;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }

            return response;
        }


        #endregion

    }



}
