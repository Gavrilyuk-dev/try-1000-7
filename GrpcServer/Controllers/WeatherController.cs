using Microsoft.AspNetCore.Mvc;

namespace GrpcServer.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    private readonly Random _random = new Random();

    [HttpGet("current")]
    public ActionResult<WeatherResponse> GetCurrentWeatherRest([FromQuery] string city)
    {
        var response = new WeatherResponse
        {
            City = city,
            Temperature = _random.Next(10, 30),
            Description = "Солнечно",
            Timestamp = DateTime.UtcNow.ToString("O")
        };

        return Ok(response);
    }

    [HttpGet("forecast")]
    public ActionResult<IEnumerable<WeatherForecast>> GetWeatherForecastRest([FromQuery] string city)
    {
        var forecasts = Enumerable.Range(0, 7).Select(i => new WeatherForecast
        {
            Date = DateTime.UtcNow.AddDays(i).ToString("yyyy-MM-dd"),
            Temperature = _random.Next(10, 30),
            Description = i % 2 == 0 ? "Солнечно" : "Облачно"
        }).ToList();

        return Ok(forecasts);
    }
}

public class WeatherResponse
{
    public string City { get; set; } = string.Empty;
    public int Temperature { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
}

public class WeatherForecast
{
    public string Date { get; set; } = string.Empty;
    public int Temperature { get; set; }
    public string Description { get; set; } = string.Empty;
}