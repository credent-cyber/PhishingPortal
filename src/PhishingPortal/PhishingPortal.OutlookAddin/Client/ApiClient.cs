using System.Net.Http.Json;

namespace PhishingPortal.OutlookAddin.Client
{
    public class ApiClient : BaseHttpClient
    {
        public ApiClient(ILogger logger, HttpClient httpClient) : base(logger, httpClient)
        {

        }

        public async Task<bool> SpamReport(string link)
        {
            try
            {
                var request = new GenericApiRequest<string>() { Param = link };
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
    }
}
