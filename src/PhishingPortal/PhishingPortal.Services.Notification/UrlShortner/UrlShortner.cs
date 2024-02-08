using PhishingPortal.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace PhishingPortal.Services.Notification.UrlShortner
{
    public class UrlShortner : IUrlShortner
    {
        public async Task<string> CallApiAsync(string urlToShorten)
        {
            string apiUrl = "https://api.encurtador.dev/encurtamentos";
            using (HttpClient client = new HttpClient())
            {

                string encodedUrl = Uri.EscapeUriString(urlToShorten);
                string requestBody = $"{{ \"url\": \"{encodedUrl}\" }}";

                HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                // Make the POST request
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response content
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonSerializer.Deserialize<ApiResponse>(responseBody);
                    return jsonResponse.urlEncurtada;
                }
                else
                {
                    return urlToShorten;
                }
            }

        }
    }

    public class ApiResponse
    {
        public string urlEncurtada { get; set; }
    }
}
