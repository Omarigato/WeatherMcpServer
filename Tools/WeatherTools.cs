using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using WeatherMcpServer.Services;
using WeatherMcpServer.Models;

namespace WeatherMcpServer.Tools;

public class WeatherTools
{
    private readonly IWeatherService _weatherService;

    public WeatherTools(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [McpServerTool]
    [Description("Gets current weather information for a specified city. Returns detailed weather data including temperature, humidity, wind speed, and more.")]
    public async Task<string> GetCityWeatherAsync(
        [Description("Name of the city to get weather for (e.g., 'London', 'New York', 'Moscow')")]
        string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return "Error: City name cannot be empty.";
        }

        var weather = await _weatherService.GetWeatherAsync(city);

        if (weather == null)
        {
            return $"Error: Could not retrieve weather information for '{city}'. Please check the city name and try again.";
        }

        return FormatWeatherInfo(weather);
    }

    [McpServerTool]
    [Description("Gets current weather information for specified coordinates (latitude and longitude).")]
    public async Task<string> GetWeatherByCoordinatesAsync(
        [Description("Latitude coordinate (e.g., 55.7558 for Moscow)")]
        double latitude,
        [Description("Longitude coordinate (e.g., 37.6176 for Moscow)")]
        double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            return "Error: Latitude must be between -90 and 90 degrees.";
        }

        if (longitude < -180 || longitude > 180)
        {
            return "Error: Longitude must be between -180 and 180 degrees.";
        }

        var weather = await _weatherService.GetWeatherByCoordinatesAsync(latitude, longitude);

        if (weather == null)
        {
            return $"Error: Could not retrieve weather information for coordinates {latitude:F4}, {longitude:F4}.";
        }

        return FormatWeatherInfo(weather);
    }

    [McpServerTool]
    [Description("Gets current weather information for a city and returns it as structured JSON data.")]
    public async Task<string> GetCityWeatherJsonAsync(
        [Description("Name of the city to get weather for")]
        string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return JsonSerializer.Serialize(new { error = "City name cannot be empty." });
        }

        var weather = await _weatherService.GetWeatherAsync(city);

        if (weather == null)
        {
            return JsonSerializer.Serialize(new { error = $"Could not retrieve weather information for '{city}'." });
        }

        return JsonSerializer.Serialize(weather, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static string FormatWeatherInfo(WeatherInfo weather)
    {
        var windDirection = GetWindDirection(weather.WindDirection);

        return $"""
        🌤️ Weather in {weather.City}, {weather.Country}
        
        🌡️ Temperature: {weather.Temperature}°C (feels like {weather.FeelsLike}°C)
        ☁️ Conditions: {weather.Description}
        💨 Wind: {weather.WindSpeed} m/s {windDirection}
        💧 Humidity: {weather.Humidity}%
        📊 Pressure: {weather.Pressure} hPa
        👁️ Visibility: {weather.Visibility / 1000.0:F1} km
        
        🌅 Sunrise: {weather.Sunrise:HH:mm}
        🌇 Sunset: {weather.Sunset:HH:mm}
        
        ⏰ Data from: {weather.DateTime:yyyy-MM-dd HH:mm} UTC
        """;
    }

    private static string GetWindDirection(int degrees)
    {
        var directions = new[]
        {
            "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE",
            "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"
        };

        var index = (int)Math.Round(degrees / 22.5) % 16;
        return $"({directions[index]})";
    }
}