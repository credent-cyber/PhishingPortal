using PhishingPortal.Server;

namespace PhishingPortal.Server
{
    public static class PhishingApiExtensions
    {

        public static string GetCurrentUser(this HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("Context not found");

            var name = httpContext.User.Claims.FirstOrDefault(o => o.Type == "name");

            if (name == null)
                throw new ArgumentException("User cannot be resolved");

            return name.Value;
        }

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
