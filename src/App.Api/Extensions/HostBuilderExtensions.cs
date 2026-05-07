using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace App.Api.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder AddSerilogConfiguration(this IHostBuilder host, IConfiguration configuration)
    {
        return host.UseSerilog((context, services, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());
    }
}
