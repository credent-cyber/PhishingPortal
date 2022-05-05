using PhishingPortal.Dto;
using System.Net.Http.Json;

namespace PhishingPortal.UI.Blazor
{
    public class WeatherClient
    {

        public WeatherClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }



        public async Task<IEnumerable<WeatherForecast>> GetWeatherTodayAsync()
        {
            var res = await HttpClient.GetAsync("/api/weatherforecast");

            if (!res.IsSuccessStatusCode)
                return Enumerable.Empty<WeatherForecast>();

            var output = await res.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>();

            if (output == null)
                return Enumerable.Empty<WeatherForecast>();

            return output;

        }

        public async Task<WeatherForecast> GetMaximumOfTheDay()
        {
            var res = await HttpClient.GetAsync("/api/weatherforcast/maximum");
            
            if (!res.IsSuccessStatusCode)
                return default(WeatherForecast);

            var output = await res.Content.ReadFromJsonAsync<WeatherForecast>();

            if(output == null)
                return default(WeatherForecast);


            return output;

        }

        public HttpClient HttpClient { get; }
    }
}
