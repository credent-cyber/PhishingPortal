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
    }
}
