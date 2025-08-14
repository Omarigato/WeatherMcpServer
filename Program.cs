using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherMcpServer.Tools;
using WeatherMcpServer.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "WeatherMcpServer/1.0");
});

builder.Services.AddSingleton<IWeatherService, WeatherService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<WeatherTools>();

await builder.Build().RunAsync();