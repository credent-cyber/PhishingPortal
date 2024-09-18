using PhishingPortal.Dto;
using System.Net.Http.Json;

namespace PhishingPortal.UI.Blazor.Client
{
    public class RequestClient : BaseHttpClient
    {
        public RequestClient(ILogger<RequestClient> logger, HttpClient httpClient) : base(logger, httpClient)
        {
        }

        public async Task<string> CreateDemoRequest(DemoRequestor demo)
        {
            try
            {
                var res = await HttpClient.PostAsJsonAsync("api/request/upsert_demorequestor", demo);

                // Ensure that the request was successful
                res.EnsureSuccessStatusCode();

                // Read the response content as a string
                var responseContent = await res.Content.ReadAsStringAsync();

                // Return the response content
                return responseContent;
            }
            catch (Exception ex)
            {
                // Log any exceptions
                Logger.LogCritical(ex, ex.Message);
                throw; // Rethrow the exception to be handled elsewhere
            }
        }


    }
}
