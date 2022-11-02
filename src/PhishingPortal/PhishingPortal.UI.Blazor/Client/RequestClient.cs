using PhishingPortal.Dto;
using System.Net.Http.Json;

namespace PhishingPortal.UI.Blazor.Client
{
    public class RequestClient : BaseHttpClient
    {
        public RequestClient(ILogger<RequestClient> logger, HttpClient httpClient) : base(logger, httpClient)
        {
        }

        public async Task<DemoRequestor> CreateDemoRequest(DemoRequestor demo)
        {
            DemoRequestor? o;
            try
            {
                var res = await HttpClient.PostAsJsonAsync("api/request/upsert_demorequestor", demo);

                res.EnsureSuccessStatusCode();

                o = await res.Content.ReadFromJsonAsync<DemoRequestor>();

            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, ex.Message);
                throw;
            }

            return o ?? demo;

        }
    }
}
