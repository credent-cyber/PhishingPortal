using PhishingPortal.Server;

namespace PhishingPortal.Server
{
    public static class PhishingApiExtensions
    {

        public static WebApplication MapPhishingApi(this WebApplication app)
        {

            // weather
            app.MapGet("api/weather-today", async (WeatherForecastService svc) => {
                var output = await svc.GetForecastAsync(DateTime.Now);
                return Results.Ok(output);
            });

            app.MapGet("api/today", async (WeatherForecastService svc) => {
                var output = await svc.GetForecastAsync(DateTime.Now);
                return Results.Ok(output);
            });

            return app;

        } 
    }
}
