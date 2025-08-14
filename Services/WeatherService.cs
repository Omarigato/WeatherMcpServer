using System.Text.Json;
using Microsoft.Extensions.Logging;
using WeatherMcpServer.Models;

namespace WeatherMcpServer.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.openweathermap.org/data/2.5";

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY") ?? "dafefe76bbed2c90f4d89aa8784436a4";
    }

    public async Task<WeatherInfo?> GetCurrentWeatherAsync(string city)
    {
        try
        {
            var url = $"{BaseUrl}/weather?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetStringAsync(url);
            var weatherData = JsonSerializer.Deserialize<OpenWeatherMapResponse>(response);
            return ConvertToWeatherInfo(weatherData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather for {City}", city);
            return null;
        }
    }

    public async Task<WeatherInfo?> GetCurrentWeatherByCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"{BaseUrl}/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetStringAsync(url);
            var weatherData = JsonSerializer.Deserialize<OpenWeatherMapResponse>(response);
            return ConvertToWeatherInfo(weatherData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather for coordinates {Lat}, {Lon}", latitude, longitude);
            return null;
        }
    }

    public async Task<WeatherForecast?> GetWeatherForecastAsync(string city, int days = 5)
    {
        try
        {
            var url = $"{BaseUrl}/forecast?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric&cnt={days * 8}";
            var response = await _httpClient.GetStringAsync(url);
            var forecastData = JsonSerializer.Deserialize<ForecastResponse>(response);
            return ConvertToWeatherForecast(forecastData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching forecast for {City}", city);
            return null;
        }
    }

    public async Task<WeatherForecast?> GetWeatherForecastByCoordinatesAsync(double latitude, double longitude, int days = 5)
    {
        try
        {
            var url = $"{BaseUrl}/forecast?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric&cnt={days * 8}";
            var response = await _httpClient.GetStringAsync(url);
            var forecastData = JsonSerializer.Deserialize<ForecastResponse>(response);
            return ConvertToWeatherForecast(forecastData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching forecast for coordinates {Lat}, {Lon}", latitude, longitude);
            return null;
        }
    }

    public async Task<WeatherAlerts?> GetWeatherAlertsAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"{BaseUrl}/onecall?lat={latitude}&lon={longitude}&appid={_apiKey}&exclude=minutely,hourly,daily";
            var response = await _httpClient.GetStringAsync(url);
            
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            
            var alerts = new WeatherAlerts
            {
                City = "Location",
                Country = "",
                Alerts = new List<WeatherAlert>()
            };

            if (root.TryGetProperty("alerts", out var alertsElement))
            {
                foreach (var alert in alertsElement.EnumerateArray())
                {
                    alerts.Alerts.Add(new WeatherAlert
                    {
                        Event = alert.GetProperty("event").GetString() ?? "",
                        Description = alert.GetProperty("description").GetString() ?? "",
                        Severity = GetSeverityLevel(alert.GetProperty("event").GetString() ?? ""),
                        Start = DateTimeOffset.FromUnixTimeSeconds(alert.GetProperty("start").GetInt64()).DateTime,
                        End = DateTimeOffset.FromUnixTimeSeconds(alert.GetProperty("end").GetInt64()).DateTime,
                        Source = alert.GetProperty("sender_name").GetString() ?? "",
                        Areas = new List<string> { alert.GetProperty("event").GetString() ?? "" }
                    });
                }
            }

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching alerts for coordinates {Lat}, {Lon}", latitude, longitude);
            return null;
        }
    }

    public async Task<AirQuality?> GetAirQualityAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"{BaseUrl}/air_pollution?lat={latitude}&lon={longitude}&appid={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var airData = JsonSerializer.Deserialize<AirPollutionResponse>(response);
            
            if (airData?.List?.FirstOrDefault() is not { } data) return null;

            return new AirQuality
            {
                City = "Location",
                Country = "",
                AirQualityIndex = data.Main.Aqi,
                QualityLevel = GetAirQualityLevel(data.Main.Aqi),
                HealthRecommendation = GetHealthRecommendation(data.Main.Aqi),
                Pollutants = new PollutantLevels
                {
                    CO = data.Components.Co,
                    NO = data.Components.No,
                    NO2 = data.Components.No2,
                    O3 = data.Components.O3,
                    SO2 = data.Components.So2,
                    PM2_5 = data.Components.Pm2_5,
                    PM10 = data.Components.Pm10,
                    NH3 = data.Components.Nh3
                },
                DateTime = DateTimeOffset.FromUnixTimeSeconds(data.Dt).DateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching air quality for coordinates {Lat}, {Lon}", latitude, longitude);
            return null;
        }
    }

    public async Task<List<WeatherInfo>> CompareWeatherAsync(List<string> cities)
    {
        var tasks = cities.Select(GetCurrentWeatherAsync);
        var results = await Task.WhenAll(tasks);
        return results.Where(w => w != null).ToList()!;
    }

    public async Task<WeatherInfo?> GetHistoricalWeatherAsync(string city, DateTime date)
    {
        try
        {
            var unixTime = ((DateTimeOffset)date).ToUnixTimeSeconds();
            var coords = await GetCoordinatesAsync(city);
            if (coords == null) return null;

            var url = $"{BaseUrl}/onecall/timemachine?lat={coords.Lat}&lon={coords.Lon}&dt={unixTime}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetStringAsync(url);
            
            using var doc = JsonDocument.Parse(response);
            var current = doc.RootElement.GetProperty("current");
            
            return new WeatherInfo
            {
                City = city,
                Country = "",
                Temperature = current.GetProperty("temp").GetDouble(),
                FeelsLike = current.GetProperty("feels_like").GetDouble(),
                Description = current.GetProperty("weather")[0].GetProperty("description").GetString() ?? "",
                MainCondition = current.GetProperty("weather")[0].GetProperty("main").GetString() ?? "",
                Humidity = current.GetProperty("humidity").GetInt32(),
                Pressure = current.GetProperty("pressure").GetDouble(),
                WindSpeed = current.GetProperty("wind_speed").GetDouble(),
                WindDirection = current.GetProperty("wind_deg").GetInt32(),
                Visibility = current.TryGetProperty("visibility", out var vis) ? vis.GetInt32() : 10000,
                CloudCover = current.GetProperty("clouds").GetInt32(),
                DateTime = date
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching historical weather for {City} on {Date}", city, date);
            return null;
        }
    }

    private async Task<Coord?> GetCoordinatesAsync(string city)
    {
        try
        {
            var url = $"{BaseUrl}/weather?q={Uri.EscapeDataString(city)}&appid={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var weatherData = JsonSerializer.Deserialize<OpenWeatherMapResponse>(response);
            return weatherData?.Coord;
        }
        catch
        {
            return null;
        }
    }

    private static WeatherInfo ConvertToWeatherInfo(OpenWeatherMapResponse? response)
    {
        if (response == null) return null!;

        return new WeatherInfo
        {
            City = response.Name,
            Country = response.Sys.Country,
            Latitude = response.Coord.Lat,
            Longitude = response.Coord.Lon,
            Temperature = response.Main.Temp,
            FeelsLike = response.Main.FeelsLike,
            MinTemperature = response.Main.TempMin,
            MaxTemperature = response.Main.TempMax,
            Description = response.Weather.FirstOrDefault()?.Description ?? "",
            MainCondition = response.Weather.FirstOrDefault()?.Main ?? "",
            Humidity = response.Main.Humidity,
            Pressure = response.Main.Pressure,
            WindSpeed = response.Wind.Speed,
            WindDirection = response.Wind.Deg,
            WindGust = response.Wind.Gust,
            Visibility = response.Visibility,
            CloudCover = response.Clouds.All,
            DateTime = DateTimeOffset.FromUnixTimeSeconds(response.Dt).DateTime,
            Sunrise = DateTimeOffset.FromUnixTimeSeconds(response.Sys.Sunrise).DateTime,
            Sunset = DateTimeOffset.FromUnixTimeSeconds(response.Sys.Sunset).DateTime,
            Timezone = TimeSpan.FromSeconds(response.Timezone).ToString()
        };
    }

    private static WeatherForecast ConvertToWeatherForecast(ForecastResponse? response)
    {
        if (response == null) return null!;

        return new WeatherForecast
        {
            City = response.City.Name,
            Country = response.City.Country,
            Items = response.List.Select(item => new ForecastItem
            {
                DateTime = DateTimeOffset.FromUnixTimeSeconds(item.Dt).DateTime,
                Temperature = item.Main.Temp,
                FeelsLike = item.Main.FeelsLike,
                MinTemperature = item.Main.TempMin,
                MaxTemperature = item.Main.TempMax,
                Description = item.Weather.FirstOrDefault()?.Description ?? "",
                MainCondition = item.Weather.FirstOrDefault()?.Main ?? "",
                Humidity = item.Main.Humidity,
                Pressure = item.Main.Pressure,
                WindSpeed = item.Wind.Speed,
                WindDirection = item.Wind.Deg,
                CloudCover = item.Clouds.All,
                PrecipitationProbability = item.Pop * 100,
                RainVolume = item.Rain?.ThreeH,
                SnowVolume = item.Snow?.ThreeH
            }).ToList()
        };
    }

    private static string GetSeverityLevel(string eventName) => eventName.ToLower() switch
    {
        var e when e.Contains("tornado") || e.Contains("hurricane") => "Extreme",
        var e when e.Contains("severe") || e.Contains("warning") => "High",
        var e when e.Contains("watch") || e.Contains("advisory") => "Medium",
        _ => "Low"
    };

    private static string GetAirQualityLevel(int aqi) => aqi switch
    {
        1 => "Good",
        2 => "Fair", 
        3 => "Moderate",
        4 => "Poor",
        5 => "Very Poor",
        _ => "Unknown"
    };

    private static string GetHealthRecommendation(int aqi) => aqi switch
    {
        1 => "Air quality is satisfactory, enjoy outdoor activities!",
        2 => "Air quality is acceptable for most people.",
        3 => "Sensitive groups should limit prolonged outdoor activities.",
        4 => "Everyone should limit outdoor activities.",
        5 => "Avoid outdoor activities and keep windows closed.",
        _ => "Unable to provide recommendation."
    };
}