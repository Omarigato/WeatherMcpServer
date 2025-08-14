using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using WeatherMcpServer.Services;
using WeatherMcpServer.Extensions;

namespace WeatherMcpServer.Tools;

public class WeatherTools
{
    private readonly IWeatherService _weatherService;

    public WeatherTools(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [McpServerTool]
    [Description("Gets current weather conditions for the specified city with detailed information including temperature, humidity, wind, and more.")]
    public async Task<string> GetCurrentWeather(
        [Description("The city name to get weather for (e.g., 'London', 'New York', 'Moscow')")] string city,
        [Description("Optional: Country code for more precise results (e.g., 'US', 'UK', 'RU')")] string? countryCode = null)
    {
        var searchCity = string.IsNullOrEmpty(countryCode) ? city : $"{city},{countryCode}";
        var weather = await _weatherService.GetCurrentWeatherAsync(searchCity);

        if (weather == null)
            return $"❌ Could not retrieve weather information for '{city}'. Please check the city name and try again.";

        return weather.FormatCurrentWeather();
    }

    [McpServerTool]
    [Description("Gets current weather information for specified coordinates (latitude and longitude).")]
    public async Task<string> GetWeatherByCoordinates(
        [Description("Latitude coordinate (e.g., 55.7558 for Moscow)")] double latitude,
        [Description("Longitude coordinate (e.g., 37.6176 for Moscow)")] double longitude)
    {
        if (latitude < -90 || latitude > 90)
            return "❌ Latitude must be between -90 and 90 degrees.";

        if (longitude < -180 || longitude > 180)
            return "❌ Longitude must be between -180 and 180 degrees.";

        var weather = await _weatherService.GetCurrentWeatherByCoordinatesAsync(latitude, longitude);

        if (weather == null)
            return $"❌ Could not retrieve weather information for coordinates {latitude:F4}, {longitude:F4}.";

        return weather.FormatCurrentWeather();
    }

    [McpServerTool]
    [Description("Gets weather forecast for the specified city. Provides up to 5 days of detailed forecast data.")]
    public async Task<string> GetWeatherForecast(
        [Description("The city name to get forecast for")] string city,
        [Description("Number of days to forecast (1-5, default: 3)")] int days = 3)
    {
        if (days < 1 || days > 5)
            return "❌ Days must be between 1 and 5.";

        var forecast = await _weatherService.GetWeatherForecastAsync(city, days);

        if (forecast == null)
            return $"❌ Could not retrieve weather forecast for '{city}'.";

        return forecast.FormatForecast(days);
    }

    [McpServerTool]
    [Description("Gets weather alerts and warnings for the specified coordinates.")]
    public async Task<string> GetWeatherAlerts(
        [Description("Latitude coordinate")] double latitude,
        [Description("Longitude coordinate")] double longitude)
    {
        var alerts = await _weatherService.GetWeatherAlertsAsync(latitude, longitude);

        if (alerts == null)
            return "❌ Could not retrieve weather alerts for the specified location.";

        if (!alerts.Alerts.Any())
            return $"✅ No active weather alerts for coordinates {latitude:F4}, {longitude:F4}.";

        return alerts.FormatAlerts();
    }

    [McpServerTool]
    [Description("Gets air quality information for the specified coordinates including pollutant levels and health recommendations.")]
    public async Task<string> GetAirQuality(
        [Description("Latitude coordinate")] double latitude,
        [Description("Longitude coordinate")] double longitude)
    {
        var airQuality = await _weatherService.GetAirQualityAsync(latitude, longitude);

        if (airQuality == null)
            return "❌ Could not retrieve air quality information for the specified location.";

        return airQuality.FormatAirQuality();
    }

    [McpServerTool]
    [Description("Compares current weather conditions between multiple cities. Great for travel planning!")]
    public async Task<string> CompareWeather(
        [Description("Comma-separated list of cities to compare (e.g., 'London,Paris,Berlin')")] string cities)
    {
        var cityList = cities.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .ToList();

        if (cityList.Count < 2)
            return "❌ Please provide at least 2 cities to compare.";

        if (cityList.Count > 5)
            return "❌ Maximum 5 cities can be compared at once.";

        var weatherData = await _weatherService.CompareWeatherAsync(cityList);

        if (!weatherData.Any())
            return "❌ Could not retrieve weather data for any of the specified cities.";

        return weatherData.FormatComparison();
    }

    [McpServerTool]
    [Description("Gets historical weather data for a specific city and date. Perfect for research and planning!")]
    public async Task<string> GetHistoricalWeather(
        [Description("The city name")] string city,
        [Description("Date in YYYY-MM-DD format (up to 5 days ago)")] string date)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return "❌ Invalid date format. Please use YYYY-MM-DD format.";

        if (parsedDate > DateTime.Today.AddDays(-1))
            return "❌ Historical weather is only available for past dates.";

        if (parsedDate < DateTime.Today.AddDays(-5))
            return "❌ Historical weather is limited to the last 5 days.";

        var weather = await _weatherService.GetHistoricalWeatherAsync(city, parsedDate);

        if (weather == null)
            return $"❌ Could not retrieve historical weather for '{city}' on {date}.";

        return weather.FormatHistoricalWeather(parsedDate);
    }

    [McpServerTool]
    [Description("Gets current weather with activity recommendations based on conditions.")]
    public async Task<string> GetWeatherWithRecommendations(
        [Description("The city name")] string city,
        [Description("Type of activity: outdoor, sports, travel, photography, or general")] string activityType = "general")
    {
        var weather = await _weatherService.GetCurrentWeatherAsync(city);

        if (weather == null)
            return $"❌ Could not retrieve weather information for '{city}'.";

        return weather.FormatWithRecommendations(activityType);
    }

    [McpServerTool]
    [Description("Gets comprehensive weather summary including current conditions, forecast, and air quality.")]
    public async Task<string> GetWeatherSummary(
        [Description("The city name")] string city)
    {
        var currentTask = _weatherService.GetCurrentWeatherAsync(city);
        var forecastTask = _weatherService.GetWeatherForecastAsync(city, 3);
        
        var current = await currentTask;
        if (current == null)
            return $"❌ Could not retrieve weather information for '{city}'.";

        var forecast = await forecastTask;
        var airQuality = await _weatherService.GetAirQualityAsync(current.Latitude, current.Longitude);

        return current.FormatComprehensiveSummary(forecast, airQuality);
    }

    [McpServerTool]
    [Description("Gets weather data as structured JSON for integration with other applications.")]
    public async Task<string> GetWeatherJson(
        [Description("The city name")] string city,
        [Description("Include forecast data (true/false)")] bool includeForecast = false)
    {
        var current = await _weatherService.GetCurrentWeatherAsync(city);
        if (current == null)
            return JsonSerializer.Serialize(new { error = $"Could not retrieve weather for '{city}'" });

        if (includeForecast)
        {
            var forecast = await _weatherService.GetWeatherForecastAsync(city, 3);
            var result = new { current, forecast };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        else
        {
            var result = new { current };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}