using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace brainbeats_backend.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class WeatherForecastController : ControllerBase {
    private static readonly string[] Summaries = new[]
    {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

    private readonly ILogger<WeatherForecastController> _logger;

    private readonly IConfiguration Configuration;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration) {
      _logger = logger;
      Configuration = configuration;
    }

    [HttpGet]
    public string Get() {
      /*
      var rng = new Random();
      return Enumerable.Range(1, 5).Select(index => new WeatherForecast {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = rng.Next(-20, 55),
        Summary = Summaries[rng.Next(Summaries.Length)]
      })
      .ToArray();
      */

      var configString = Configuration["test_env_string"];

      Console.WriteLine(configString);

      return new string($"Test Env String: {configString}");
    }
  }
}
