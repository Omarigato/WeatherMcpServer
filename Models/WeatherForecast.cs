namespace WeatherMcpServer.Models;

public class WeatherForecast
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<ForecastItem> Items { get; set; } = new();
}

public class ForecastItem
{
    public DateTime DateTime { get; set; }
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public double MinTemperature { get; set; }
    public double MaxTemperature { get; set; }
    public string Description { get; set; } = string.Empty;
    public string MainCondition { get; set; } = string.Empty;
    public int Humidity { get; set; }
    public double Pressure { get; set; }
    public double WindSpeed { get; set; }
    public int WindDirection { get; set; }
    public int CloudCover { get; set; }
    public double? PrecipitationProbability { get; set; }
    public double? RainVolume { get; set; }
    public double? SnowVolume { get; set; }
}