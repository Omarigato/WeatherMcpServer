using WeatherMcpServer.Models;

namespace WeatherMcpServer.Services;

public interface IWeatherService
{
    Task<WeatherInfo?> GetCurrentWeatherAsync(string city);
    Task<WeatherInfo?> GetCurrentWeatherByCoordinatesAsync(double latitude, double longitude);
    Task<WeatherForecast?> GetWeatherForecastAsync(string city, int days = 5);
    Task<WeatherForecast?> GetWeatherForecastByCoordinatesAsync(double latitude, double longitude, int days = 5);
    Task<WeatherAlerts?> GetWeatherAlertsAsync(double latitude, double longitude);
    Task<AirQuality?> GetAirQualityAsync(double latitude, double longitude);
    Task<List<WeatherInfo>> CompareWeatherAsync(List<string> cities);
    Task<WeatherInfo?> GetHistoricalWeatherAsync(string city, DateTime date);
}