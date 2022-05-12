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
    }
}
