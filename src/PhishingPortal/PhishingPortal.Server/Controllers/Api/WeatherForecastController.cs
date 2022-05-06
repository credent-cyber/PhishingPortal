using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhishingPortal.Dto;

namespace PhishingPortal.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class WeatherForecastController : Controller
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHttpContextAccessor httpContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger , IHttpContextAccessor httpContext)
        {
            _logger = logger;
            this.httpContext = httpContext;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("Maximum")]
        public WeatherForecast GetMaxOfTheDay()
        {
            return new WeatherForecast() { 
             Date = DateTime.Now,
             TemperatureC = -23,
              Summary = "Cold, with cloudly and little rain"
            };
        }
    }
}