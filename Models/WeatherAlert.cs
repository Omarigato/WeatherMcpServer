namespace WeatherMcpServer.Models;

public class WeatherAlert
{
    public string Event { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Source { get; set; } = string.Empty;
    public List<string> Areas { get; set; } = new();
}

public class WeatherAlerts
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<WeatherAlert> Alerts { get; set; } = new();
}