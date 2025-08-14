using System.Text.Json.Serialization;

namespace WeatherMcpServer.Models;

public class OpenWeatherMapResponse
{
    [JsonPropertyName("coord")]
    public Coord Coord { get; set; } = new();

    [JsonPropertyName("weather")]
    public List<Weather> Weather { get; set; } = new();

    [JsonPropertyName("main")]
    public Main Main { get; set; } = new();

    [JsonPropertyName("visibility")]
    public int Visibility { get; set; }

    [JsonPropertyName("wind")]
    public Wind Wind { get; set; } = new();

    [JsonPropertyName("clouds")]
    public Clouds Clouds { get; set; } = new();

    [JsonPropertyName("dt")]
    public long Dt { get; set; }

    [JsonPropertyName("sys")]
    public Sys Sys { get; set; } = new();

    [JsonPropertyName("timezone")]
    public int Timezone { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ForecastResponse
{
    [JsonPropertyName("list")]
    public List<ForecastData> List { get; set; } = new();

    [JsonPropertyName("city")]
    public City City { get; set; } = new();
}

public class ForecastData
{
    [JsonPropertyName("dt")]
    public long Dt { get; set; }

    [JsonPropertyName("main")]
    public Main Main { get; set; } = new();

    [JsonPropertyName("weather")]
    public List<Weather> Weather { get; set; } = new();

    [JsonPropertyName("clouds")]
    public Clouds Clouds { get; set; } = new();

    [JsonPropertyName("wind")]
    public Wind Wind { get; set; } = new();

    [JsonPropertyName("pop")]
    public double Pop { get; set; }

    [JsonPropertyName("rain")]
    public Rain? Rain { get; set; }

    [JsonPropertyName("snow")]
    public Snow? Snow { get; set; }
}

public class City
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("coord")]
    public Coord Coord { get; set; } = new();

    [JsonPropertyName("timezone")]
    public int Timezone { get; set; }
}

public class AirPollutionResponse
{
    [JsonPropertyName("list")]
    public List<AirPollutionData> List { get; set; } = new();
}

public class AirPollutionData
{
    [JsonPropertyName("main")]
    public AirMain Main { get; set; } = new();

    [JsonPropertyName("components")]
    public Components Components { get; set; } = new();

    [JsonPropertyName("dt")]
    public long Dt { get; set; }
}

public class AirMain
{
    [JsonPropertyName("aqi")]
    public int Aqi { get; set; }
}

public class Components
{
    [JsonPropertyName("co")]
    public double Co { get; set; }

    [JsonPropertyName("no")]
    public double No { get; set; }

    [JsonPropertyName("no2")]
    public double No2 { get; set; }

    [JsonPropertyName("o3")]
    public double O3 { get; set; }

    [JsonPropertyName("so2")]
    public double So2 { get; set; }

    [JsonPropertyName("pm2_5")]
    public double Pm2_5 { get; set; }

    [JsonPropertyName("pm10")]
    public double Pm10 { get; set; }

    [JsonPropertyName("nh3")]
    public double Nh3 { get; set; }
}

public class Coord
{
    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }
}

public class Weather
{
    [JsonPropertyName("main")]
    public string Main { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class Main
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }

    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }

    [JsonPropertyName("temp_min")]
    public double TempMin { get; set; }

    [JsonPropertyName("temp_max")]
    public double TempMax { get; set; }

    [JsonPropertyName("pressure")]
    public double Pressure { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

public class Wind
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }

    [JsonPropertyName("deg")]
    public int Deg { get; set; }

    [JsonPropertyName("gust")]
    public double? Gust { get; set; }
}

public class Clouds
{
    [JsonPropertyName("all")]
    public int All { get; set; }
}

public class Sys
{
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("sunrise")]
    public long Sunrise { get; set; }

    [JsonPropertyName("sunset")]
    public long Sunset { get; set; }
}

public class Rain
{
    [JsonPropertyName("3h")]
    public double ThreeH { get; set; }
}

public class Snow
{
    [JsonPropertyName("3h")]
    public double ThreeH { get; set; }
}