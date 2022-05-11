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
                var res = await HttpClient.PostAsJsonAsync("api/Onboarding/Register", tenant);

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

        
        public async Task<Tenant> GetTenantByUniqueId(string uniqueId)
        {
            var res = await HttpClient.GetAsync($"api/onboarding/TenantByUniqueId?uniqueId={uniqueId}");
            res.EnsureSuccessStatusCode();
            var val = await res.Content.ReadFromJsonAsync<Tenant>() ;
            return val;
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

        /// <summary>
        /// RegistrationConfirmation
        /// </summary>
        /// <param name="uniqueId">Tenant UniqueID</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<Tenant>> Confirmation(TenantConfirmationRequest request)
        {
            ApiResponse<Tenant> content;
            var res = await HttpClient.PostAsJsonAsync($"api/onboarding/confirm", request);

            res.EnsureSuccessStatusCode();
            content = await res.Content.ReadFromJsonAsync<ApiResponse<Tenant>>();
            return content;
        }

        /// <summary>
        /// DomainConfirmation
        /// </summary>
        /// <param name="uniqueId">Tenant UniqueID</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<Tenant>> DomainConfirmation(DomainVerificationRequest domain)
        {
            ApiResponse<Tenant> content;
            var res = await HttpClient.PostAsJsonAsync($"api/onboarding/ConfirmDomain", domain);

            res.EnsureSuccessStatusCode();
            content = await res.Content.ReadFromJsonAsync<ApiResponse<Tenant>>();
            return content;
        }

        /// <summary>
        /// CreateTenandAdminUser
        /// </summary>
        /// <param name="uniqueId">Tenant UniqueID</param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> CreateTenandAdminUser(string uniqueId, string email, string password, string confirmPass)
        {
            ApiResponse<bool> content = new ApiResponse<bool>();
            var res = await HttpClient.PostAsJsonAsync($"api/onboarding/CreateDefaultUser", new TenantAdminUser
            {
                TenantUniqueId = uniqueId,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPass
            }) ;

            res.EnsureSuccessStatusCode();
            content = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return content?.Result ?? false;
        }

    }
}
