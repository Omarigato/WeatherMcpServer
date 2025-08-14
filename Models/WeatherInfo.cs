public class WeatherInfo
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
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
    public double? WindGust { get; set; }
    public int Visibility { get; set; }
    public int CloudCover { get; set; }
    public double UvIndex { get; set; }
    public DateTime DateTime { get; set; }
    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }
    public string Timezone { get; set; } = string.Empty;
}