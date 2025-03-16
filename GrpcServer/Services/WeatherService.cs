using Grpc.Core;
using WeatherServer;

namespace GrpcServer.Services;

public class WeatherServiceImpl : WeatherService.WeatherServiceBase
{
    private readonly Random _random = new Random();

    public override Task<WeatherResponse> GetCurrentWeather(
        WeatherRequest request, ServerCallContext context)
    {
        return Task.FromResult(new WeatherResponse
        {
            City = request.City,
            Temperature = _random.Next(10, 30),
            Description = "Солнечно",
            Timestamp = DateTime.UtcNow.ToString("O")
        });
    }

    public override async Task MonitorWeather(
        WeatherRequest request,
        IServerStreamWriter<WeatherResponse> responseStream,
        ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var weather = new WeatherResponse
            {
                City = request.City,
                Temperature = _random.Next(10, 30),
                Description = "Солнечно",
                Timestamp = DateTime.UtcNow.ToString("O")
            };

            await responseStream.WriteAsync(weather);
            await Task.Delay(1000, context.CancellationToken);
        }
    }

    public override Task<WeatherForecastResponse> GetWeatherForecast(
        WeatherRequest request, ServerCallContext context)
    {
        var forecasts = new List<WeatherForecast>();

        for (int i = 0; i < 7; i++)
        {
            forecasts.Add(new WeatherForecast
            {
                Date = DateTime.UtcNow.AddDays(i).ToString("yyyy-MM-dd"),
                Temperature = _random.Next(10, 30),
                Description = i % 2 == 0 ? "Солнечно" : "Облачно"
            });
        }

        var response = new WeatherForecastResponse { Forecasts = { forecasts } };
        return Task.FromResult(response);
    }
}