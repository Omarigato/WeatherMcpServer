using System.Text;
using WeatherMcpServer.Models;

namespace WeatherMcpServer.Extensions;

public static class WeatherExtensions
{
    public static string FormatCurrentWeather(this WeatherInfo weather)
    {
        var windDirection = GetWindDirection(weather.WindDirection);
        var uvAdvice = GetUVAdvice(weather.UvIndex);

        return $"""
        🌤️ **Current Weather in {weather.City}, {weather.Country}**
        
        🌡️ **Temperature:** {weather.Temperature}°C (feels like {weather.FeelsLike}°C)
        📊 **Range:** {weather.MinTemperature}°C - {weather.MaxTemperature}°C
        ☁️ **Conditions:** {CapitalizeFirst(weather.Description)}
        
        💨 **Wind:** {weather.WindSpeed} m/s {windDirection}{(weather.WindGust.HasValue ? $" (gusts: {weather.WindGust}m/s)" : "")}
        💧 **Humidity:** {weather.Humidity}%
        📊 **Pressure:** {weather.Pressure} hPa
        👁️ **Visibility:** {weather.Visibility / 1000.0:F1} km
        ☁️ **Cloud Cover:** {weather.CloudCover}%
        
        🌅 **Sunrise:** {weather.Sunrise:HH:mm} | 🌇 **Sunset:** {weather.Sunset:HH:mm}
        
        ⏰ **Updated:** {weather.DateTime:yyyy-MM-dd HH:mm} UTC
        📍 **Coordinates:** {weather.Latitude:F4}, {weather.Longitude:F4}
        """;
    }

    public static string FormatForecast(this WeatherForecast forecast, int days)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"📅 **{days}-Day Weather Forecast for {forecast.City}, {forecast.Country}**\n");

        var dailyGroups = forecast.Items
            .GroupBy(item => item.DateTime.Date)
            .Take(days)
            .ToList();

        foreach (var day in dailyGroups)
        {
            var dayItems = day.ToList();
            var minTemp = dayItems.Min(x => x.MinTemperature);
            var maxTemp = dayItems.Max(x => x.MaxTemperature);
            var avgHumidity = dayItems.Average(x => x.Humidity);
            var maxWindSpeed = dayItems.Max(x => x.WindSpeed);
            var avgPrecipitation = dayItems.Average(x => x.PrecipitationProbability ?? 0);
            var dominantCondition = dayItems
                .GroupBy(x => x.MainCondition)
                .OrderByDescending(g => g.Count())
                .First().Key;

            var dayName = day.Key == DateTime.Today ? "Today" :
                         day.Key == DateTime.Today.AddDays(1) ? "Tomorrow" :
                         day.Key.ToString("dddd");

            sb.AppendLine($"**{dayName} ({day.Key:MMM dd})**");
            sb.AppendLine($"🌡️ {minTemp:F0}°C - {maxTemp:F0}°C | ☁️ {dominantCondition}");
            sb.AppendLine($"💧 Humidity: {avgHumidity:F0}% | 💨 Wind: {maxWindSpeed:F1} m/s");
            if (avgPrecipitation > 0)
                sb.AppendLine($"🌧️ Rain chance: {avgPrecipitation:F0}%");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string FormatAlerts(this WeatherAlerts alerts)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"⚠️ **Weather Alerts for {alerts.City}**\n");

        foreach (var alert in alerts.Alerts)
        {
            var severityIcon = alert.Severity switch
            {
                "Extreme" => "🔴",
                "High" => "🟠",
                "Medium" => "🟡",
                _ => "🔵"
            };

            sb.AppendLine($"{severityIcon} **{alert.Event}** ({alert.Severity})");
            sb.AppendLine($"📅 {alert.Start:MMM dd, HH:mm} - {alert.End:MMM dd, HH:mm}");
            sb.AppendLine($"📝 {alert.Description}");
            sb.AppendLine($"📡 Source: {alert.Source}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string FormatAirQuality(this AirQuality airQuality)
    {
        var qualityIcon = airQuality.AirQualityIndex switch
        {
            1 => "🟢",
            2 => "🟡", 
            3 => "🟠",
            4 => "🔴",
            5 => "🟣",
            _ => "⚪"
        };

        return $"""
        🌬️ **Air Quality Information**
        
        {qualityIcon} **Overall Quality:** {airQuality.QualityLevel} (AQI: {airQuality.AirQualityIndex})
        💡 **Health Advice:** {airQuality.HealthRecommendation}
        
        **Pollutant Levels (μg/m³):**
        • PM2.5: {airQuality.Pollutants.PM2_5:F1} | PM10: {airQuality.Pollutants.PM10:F1}
        • NO₂: {airQuality.Pollutants.NO2:F1} | O₃: {airQuality.Pollutants.O3:F1}
        • SO₂: {airQuality.Pollutants.SO2:F1} | CO: {airQuality.Pollutants.CO:F1}
        
        ⏰ **Measured:** {airQuality.DateTime:yyyy-MM-dd HH:mm} UTC
        """;
    }

    public static string FormatComparison(this List<WeatherInfo> weatherData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🌍 **Weather Comparison**\n");

        var sortedByTemp = weatherData.OrderByDescending(w => w.Temperature).ToList();
        
        sb.AppendLine("🌡️ **Temperature Ranking:**");
        for (int i = 0; i < sortedByTemp.Count; i++)
        {
            var weather = sortedByTemp[i];
            var medal = i switch { 0 => "🥇", 1 => "🥈", 2 => "🥉", _ => "🔹" };
            sb.AppendLine($"{medal} {weather.City}: {weather.Temperature}°C ({weather.Description})");
        }

        sb.AppendLine("\n💨 **Wind Conditions:**");
        foreach (var weather in weatherData)
        {
            var windIcon = weather.WindSpeed switch
            {
                < 3 => "🍃",
                < 8 => "💨", 
                < 15 => "🌬️",
                _ => "💨"
            };
            sb.AppendLine($"{windIcon} {weather.City}: {weather.WindSpeed} m/s");
        }

        sb.AppendLine("\n💧 **Humidity Levels:**");
        foreach (var weather in weatherData)
        {
            var humidityIcon = weather.Humidity switch
            {
                < 30 => "🏜️",
                < 60 => "🌤️",
                < 80 => "💧",
                _ => "💦"
            };
            sb.AppendLine($"{humidityIcon} {weather.City}: {weather.Humidity}%");
        }

        return sb.ToString();
    }

    public static string FormatHistoricalWeather(this WeatherInfo weather, DateTime date)
    {
        return $"""
        📊 **Historical Weather for {weather.City}**
        📅 **Date:** {date:dddd, MMMM dd, yyyy}
        
        🌡️ **Temperature:** {weather.Temperature}°C (felt like {weather.FeelsLike}°C)
        ☁️ **Conditions:** {CapitalizeFirst(weather.Description)}
        💨 **Wind:** {weather.WindSpeed} m/s {GetWindDirection(weather.WindDirection)}
        💧 **Humidity:** {weather.Humidity}%
        📊 **Pressure:** {weather.Pressure} hPa
        ☁️ **Cloud Cover:** {weather.CloudCover}%
        
        📈 **Comparison to today:** Use current weather to see how conditions have changed!
        """;
    }

    public static string FormatWithRecommendations(this WeatherInfo weather, string activityType)
    {
        var recommendations = GetActivityRecommendations(weather, activityType);
        
        return $"""
        {weather.FormatCurrentWeather()}
        
        🎯 **{CapitalizeFirst(activityType)} Recommendations:**
        {recommendations}
        """;
    }

    public static string FormatComprehensiveSummary(this WeatherInfo current, WeatherForecast? forecast, AirQuality? airQuality)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("📋 **Comprehensive Weather Summary**\n");
        sb.AppendLine(current.FormatCurrentWeather());
        
        if (forecast != null)
        {
            sb.AppendLine("\n" + forecast.FormatForecast(3));
        }
        
        if (airQuality != null)
        {
            sb.AppendLine(airQuality.FormatAirQuality());
        }

        return sb.ToString();
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

    private static string GetUVAdvice(double uvIndex) => uvIndex switch
    {
        < 3 => "☀️ Low UV - Safe for outdoor activities",
        < 6 => "⚡ Moderate UV - Use sunscreen",
        < 8 => "🔥 High UV - Seek shade during midday",
        < 11 => "⚠️ Very High UV - Avoid sun exposure",
        _ => "🚨 Extreme UV - Stay indoors"
    };

    private static string GetActivityRecommendations(WeatherInfo weather, string activityType)
    {
        return activityType.ToLower() switch
        {
            "outdoor" => GetOutdoorRecommendations(weather),
            "sports" => GetSportsRecommendations(weather),
            "travel" => GetTravelRecommendations(weather),
            "photography" => GetPhotographyRecommendations(weather),
            _ => GetGeneralRecommendations(weather)
        };
    }

    private static string GetOutdoorRecommendations(WeatherInfo weather)
    {
        var recommendations = new List<string>();

        if (weather.Temperature > 25)
            recommendations.Add("🌞 Great for outdoor activities! Stay hydrated.");
        else if (weather.Temperature < 5)
            recommendations.Add("🧥 Bundle up warmly for outdoor activities.");

        if (weather.WindSpeed > 10)
            recommendations.Add("💨 Windy conditions - secure loose items.");

        if (weather.Humidity > 80)
            recommendations.Add("💦 High humidity - take frequent breaks.");

        if (weather.CloudCover < 30)
            recommendations.Add("☀️ Clear skies - perfect for outdoor fun!");

        return recommendations.Any() ? string.Join("\n", recommendations) : "🌤️ Suitable conditions for outdoor activities.";
    }

    private static string GetSportsRecommendations(WeatherInfo weather)
    {
        var recommendations = new List<string>();

        if (weather.Temperature >= 15 && weather.Temperature <= 25)
            recommendations.Add("🏃 Perfect temperature for sports activities!");
        else if (weather.Temperature > 30)
            recommendations.Add("🥵 Hot weather - exercise early morning or evening.");

        if (weather.WindSpeed > 15)
            recommendations.Add("💨 Strong winds may affect ball sports.");

        if (weather.Description.Contains("rain"))
            recommendations.Add("🏠 Consider indoor sports due to rain.");

        return recommendations.Any() ? string.Join("\n", recommendations) : "⚽ Good conditions for sports.";
    }

    private static string GetTravelRecommendations(WeatherInfo weather)
    {
        var recommendations = new List<string>();

        if (weather.Visibility < 5000)
            recommendations.Add("🌫️ Low visibility - drive carefully.");

        if (weather.WindSpeed > 20)
            recommendations.Add("✈️ Strong winds may affect flights.");

        if (weather.Description.Contains("storm"))
            recommendations.Add("⛈️ Severe weather - consider delaying travel.");

        return recommendations.Any() ? string.Join("\n", recommendations) : "🚗 Good conditions for travel.";
    }

    private static string GetPhotographyRecommendations(WeatherInfo weather)
    {
        var recommendations = new List<string>();

        if (weather.CloudCover >= 50 && weather.CloudCover <= 80)
            recommendations.Add("📸 Great for portrait photography - soft natural light!");

        if (weather.CloudCover < 20)
            recommendations.Add("🌅 Perfect for landscape photography during golden hour.");

        if (weather.Description.Contains("storm"))
            recommendations.Add("⚡ Dramatic storm photography opportunities (stay safe!).");

        return recommendations.Any() ? string.Join("\n", recommendations) : "📷 Good lighting conditions for photography.";
    }

    private static string GetGeneralRecommendations(WeatherInfo weather)
    {
        var recommendations = new List<string>();

        if (weather.Temperature < 0)
            recommendations.Add("🧊 Freezing temperatures - watch for ice.");

        if (weather.Humidity < 30)
            recommendations.Add("💧 Low humidity - moisturize skin.");

        if (weather.Pressure < 1000)
            recommendations.Add("📉 Low pressure - weather may change soon.");

        return recommendations.Any() ? string.Join("\n", recommendations) : "🌤️ Pleasant weather conditions.";
    }

    private static string CapitalizeFirst(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input[1..];
    }
}