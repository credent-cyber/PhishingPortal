using PhishingPortal.Dto;
using System.Text.Json;
using System.Net.Http.Json;

namespace PhishingPortal.UI.Blazor.Client
{
    public class TenantAdminClient : BaseHttpClient
    {
        public TenantAdminClient(ILogger<TenantAdminClient> logger, HttpClient httpClient)
            : base(logger, httpClient)
        {

        }

        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {
            Tenant? o;
            try
            {
                var res = await HttpClient.PostAsJsonAsync("api/Onboarding", tenant);

                res.EnsureSuccessStatusCode();

                o = await res.Content.ReadFromJsonAsync<Tenant>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return o ?? tenant;

        }
    }
}
