namespace WeatherMcpServer.Models;

public class AirQuality
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int AirQualityIndex { get; set; }
    public string QualityLevel { get; set; } = string.Empty;
    public string HealthRecommendation { get; set; } = string.Empty;
    public PollutantLevels Pollutants { get; set; } = new();
    public DateTime DateTime { get; set; }
}

public class PollutantLevels
{
    public double CO { get; set; }      // Carbon monoxide
    public double NO { get; set; }      // Nitric oxide
    public double NO2 { get; set; }     // Nitrogen dioxide
    public double O3 { get; set; }      // Ozone
    public double SO2 { get; set; }     // Sulphur dioxide
    public double PM2_5 { get; set; }   // Fine particles
    public double PM10 { get; set; }    // Coarse particles
    public double NH3 { get; set; }     // Ammonia
}