using PhishingPortal.Dto;
using System.Net.Http.Json;

namespace PhishingPortal.UI.Blazor.Client
{
    public class TenantClient : BaseHttpClient
    {
        public TenantClient(ILogger<TenantClient> logger, HttpClient httpClient)
            :base(logger, httpClient)
        {

        }

        public async Task<IEnumerable<Campaign>> GetCampaignsAsync(int pageIndex = 0, int pageSize = 10)
        {
            IEnumerable<Campaign> campaigns;
            try
            {
                var res = await HttpClient.GetAsync("api/Tenant/Campaigns?pageIndex={pageIndex}&pageSize={pageSize}");

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
            List<CampaignTemplate>  templates = Enumerable.Empty<CampaignTemplate>().ToList();
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
                    url += $"/{ type.Value }";
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
            catch(Exception ex)
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


    }
}
