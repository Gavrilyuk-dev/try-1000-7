using Grpc.Core;
using Grpc.Net.Client;
using WeatherClient;
using System.Net.Http.Json;
using System.Diagnostics;

const int messageCount = 10000;
const string city = "Москва";

var httpHandler = new HttpClientHandler();
            
httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

using var channel = GrpcChannel.ForAddress("https://localhost:7276", new GrpcChannelOptions
{
    HttpHandler = httpHandler
});

var client = new WeatherService.WeatherServiceClient(channel);
var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7276") };

Console.WriteLine($"Тестируем скорость передачи {messageCount} сообщений...\n");
await TestGrpcPerformance(client);
await TestRestPerformance(httpClient);

Console.WriteLine($"Тестируем скорость передачи массивов с {messageCount} элементов...\n");
await TestGrpcArrayPerformance(client);
await TestRestArrayPerformance(httpClient);

Console.WriteLine("\nРаботает gRPC:");
var currentWeather = await client.GetCurrentWeatherAsync(
    new WeatherRequest { City = "Москва" });
Console.WriteLine($"Текущая погода: {currentWeather.City}, " +
                  $"{currentWeather.Temperature}°C, {currentWeather.Description}");

Console.WriteLine("\nРаботает REST API:");
var currentWeatherRest = await httpClient.GetFromJsonAsync<WeatherResponse>("api/weather/current?city=Москва");
if (currentWeatherRest != null)
{
    Console.WriteLine($"Текущая погода: {currentWeatherRest.City}, {currentWeatherRest.Temperature}°C, {currentWeatherRest.Description}");
}


Console.WriteLine("\nПрогноз погоды на неделю (gRPC):");
var forecastResponse = await client.GetWeatherForecastAsync(new WeatherRequest { City = "Москва" });

foreach (var forecast in forecastResponse.Forecasts)
{
    Console.WriteLine($"{forecast.Date}: {forecast.Temperature}°C, {forecast.Description}");
}

Console.WriteLine("\nПрогноз погоды на неделю (REST API):");
var forecastRest = await httpClient.GetFromJsonAsync<List<WeatherForecast>>("api/weather/forecast?city=Москва");
if (forecastRest != null)
{
    foreach (var forecast in forecastRest)
    {
        Console.WriteLine($"Дата: {forecast.Date}, Температура: {forecast.Temperature}°C, {forecast.Description}");
    }
}

Console.WriteLine("\nНачинаем мониторинг погоды...");
using var monitoring = client.MonitorWeather(new WeatherRequest { City = "Москва" });

try
{
    await foreach (var weather in monitoring.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Обновление: {weather.City}, " +
                          $"{weather.Temperature}°C, {weather.Timestamp}");
    }
}
catch (RpcException ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}

//методы тестирования скорости передачи 10000 сообщений:
async Task TestGrpcPerformance(WeatherService.WeatherServiceClient client)
{
    var stopwatch = Stopwatch.StartNew();

    for (int i = 0; i < messageCount; i++)
    {
        await client.GetCurrentWeatherAsync(new WeatherRequest { City = city });
    }

    stopwatch.Stop();
    Console.WriteLine($"gRPC: {messageCount} запросов выполнено за {stopwatch.ElapsedMilliseconds} мс");
}

async Task TestRestPerformance(HttpClient client)
{
    var stopwatch = Stopwatch.StartNew();

    for (int i = 0; i < messageCount; i++)
    {
        await client.GetFromJsonAsync<WeatherResponse>($"api/weather/current?city={city}");
    }

    stopwatch.Stop();
    Console.WriteLine($"REST: {messageCount} запросов выполнено за {stopwatch.ElapsedMilliseconds} мс");
}

//методы тестирования скорости передачи массива из 10000 элементов:
async Task TestGrpcArrayPerformance(WeatherService.WeatherServiceClient client)
{
    var stopwatch = Stopwatch.StartNew();

    var weatherArray = new WeatherRequest[messageCount];
    for (int i = 0; i < messageCount; i++)
    {
        weatherArray[i] = new WeatherRequest { City = city };
    }

    foreach (var request in weatherArray)
    {
        await client.GetCurrentWeatherAsync(request);
    }

    stopwatch.Stop();
    Console.WriteLine($"gRPC (массив): {messageCount} запросов выполнено за {stopwatch.ElapsedMilliseconds} мс");
}

async Task TestRestArrayPerformance(HttpClient client)
{
    var stopwatch = Stopwatch.StartNew();

    var weatherArray = new WeatherRequest[messageCount];
    for (int i = 0; i < messageCount; i++)
    {
        weatherArray[i] = new WeatherRequest { City = city };
    }

    foreach (var request in weatherArray)
    {
        var response = await client.GetFromJsonAsync<WeatherResponse>($"api/weather/current?city={request.City}");
    }

    stopwatch.Stop();
    Console.WriteLine($"REST (массив): {messageCount} запросов выполнено за {stopwatch.ElapsedMilliseconds} мс");
}