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

        /// <summary>
        /// CreateTenantAsync
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
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

        /// <summary>
        /// GetAllAsync
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<List<Tenant>> GetAllAsync(int pageIndex = 0, int pageSize = 1000)
        {
            List<Tenant>? list = new List<Tenant>();
            try
            {
                var res = await HttpClient.GetAsync($"api/onboarding?pageIndex={pageIndex}&pageSize={pageSize}");

                res.EnsureSuccessStatusCode();

                list = await res.Content.ReadFromJsonAsync<List<Tenant>>() ;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }

            return  list ?? Enumerable.Empty<Tenant>().ToList();
        }

        /// <summary>
        /// Provision
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<bool> Provision(int tenantId, string connectionString)
        {
            bool result = false;

            var payload = new ProvisionTenantRequest
            {
                TenantId = tenantId,
                ConnectionString = connectionString
            };

            try
            {
                var res = await HttpClient.PostAsJsonAsync($"api/onboarding/provision", payload);
                res.EnsureSuccessStatusCode();

                if(res.IsSuccessStatusCode)
                    result = true;


            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
            return result;
        }
    }
}
