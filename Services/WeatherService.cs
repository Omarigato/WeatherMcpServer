using System.Text.Json;
using WeatherMcpServer.Models;
using Microsoft.Extensions.Logging;

namespace WeatherMcpServer.Services;

public interface IWeatherService
{
    Task<WeatherInfo?> GetWeatherAsync(string city);
    Task<WeatherInfo?> GetWeatherByCoordinatesAsync(double latitude, double longitude);
}

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Получаем API ключ из переменной окружения
        _apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY") ?? string.Empty;

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("OpenWeatherMap API key not found. Set OPENWEATHER_API_KEY environment variable.");
        }
    }

    public async Task<WeatherInfo?> GetWeatherAsync(string city)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("API key is not configured");
            return CreateMockWeather(city);
        }

        try
        {
            var url = $"{BaseUrl}?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric";

            _logger.LogInformation("Fetching weather for city: {City}", city);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch weather data: {StatusCode} {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<OpenWeatherMapResponse>(jsonContent);

            if (weatherData == null)
            {
                _logger.LogError("Failed to deserialize weather response");
                return null;
            }

            return MapToWeatherInfo(weatherData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather for city: {City}", city);
            return null;
        }
    }

    public async Task<WeatherInfo?> GetWeatherByCoordinatesAsync(double latitude, double longitude)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("API key is not configured");
            return CreateMockWeather($"{latitude:F2}, {longitude:F2}");
        }

        try
        {
            var url = $"{BaseUrl}?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";

            _logger.LogInformation("Fetching weather for coordinates: {Lat}, {Lon}", latitude, longitude);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch weather data: {StatusCode} {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<OpenWeatherMapResponse>(jsonContent);

            if (weatherData == null)
            {
                _logger.LogError("Failed to deserialize weather response");
                return null;
            }

            return MapToWeatherInfo(weatherData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather for coordinates: {Lat}, {Lon}", latitude, longitude);
            return null;
        }
    }

    private static WeatherInfo MapToWeatherInfo(OpenWeatherMapResponse response)
    {
        var weather = response.Weather.FirstOrDefault();

        return new WeatherInfo
        {
            City = response.Name,
            Country = response.Sys.Country,
            Temperature = Math.Round(response.Main.Temp, 1),
            FeelsLike = Math.Round(response.Main.FeelsLike, 1),
            Description = weather?.Description ?? "Unknown",
            MainCondition = weather?.Main ?? "Unknown",
            Humidity = response.Main.Humidity,
            Pressure = Math.Round(response.Main.Pressure, 1),
            WindSpeed = Math.Round(response.Wind.Speed, 1),
            WindDirection = response.Wind.Deg,
            Visibility = response.Visibility,
            DateTime = DateTimeOffset.FromUnixTimeSeconds(response.Dt).DateTime,
            Sunrise = DateTimeOffset.FromUnixTimeSeconds(response.Sys.Sunrise).DateTime,
            Sunset = DateTimeOffset.FromUnixTimeSeconds(response.Sys.Sunset).DateTime
        };
    }

    private static WeatherInfo CreateMockWeather(string location)
    {
        var mockConditions = new[] { "sunny", "cloudy", "rainy", "partly cloudy", "clear" };
        var random = Random.Shared;
        var condition = mockConditions[random.Next(mockConditions.Length)];

        return new WeatherInfo
        {
            City = location,
            Country = "Mock",
            Temperature = Math.Round(random.NextDouble() * 30 + 5, 1), // 5-35°C
            FeelsLike = Math.Round(random.NextDouble() * 30 + 5, 1),
            Description = condition,
            MainCondition = condition,
            Humidity = random.Next(30, 90),
            Pressure = Math.Round(random.NextDouble() * 50 + 1000, 1), // 1000-1050 hPa
            WindSpeed = Math.Round(random.NextDouble() * 20, 1), // 0-20 m/s
            WindDirection = random.Next(0, 360),
            Visibility = random.Next(5000, 15000),
            DateTime = DateTime.UtcNow,
            Sunrise = DateTime.UtcNow.Date.AddHours(6),
            Sunset = DateTime.UtcNow.Date.AddHours(20)
        };
    }
}