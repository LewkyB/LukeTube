using LukeTube.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.Threading.Tasks;
using NLog;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace LukeTube;
#pragma warning disable CS1591 // used for disabling xml comment warning for swagger
public class Program
{
    public static async Task Main(string[] args)
    {
        var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        try
        {
            logger.Debug("init main");

            var host = CreateHostBuilder(args).Build();

            // seed database at startup
            await RunSeedingAsync(host);

            await host.RunAsync();
        }
        catch (Exception exception)
        {
            //NLog: catch setup errors
            logger.Error(exception, "Stopped program because of exception");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            NLog.LogManager.Shutdown();
        }
    }

    // right now this isn't used to do any seeding of data, it is only setting up tables for miniprofiler
    private static async Task RunSeedingAsync(IHost host)
    {
        var scopeFactory = host.Services.GetService<IServiceScopeFactory>();
        using (var scope = scopeFactory.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetService<IDatabaseSeeder>();
            await seeder.InitializeAsync();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddOpenTelemetry(o =>
                {
                    o.ConfigureResource(r =>
                    {
                        r.AddService(Telemetry.ServiceName, Telemetry.ServiceVersion);
                    });
                    o.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri("http://otel_collector:4317");
                    });
                    o.IncludeScopes = true;
                    o.IncludeFormattedMessage = true;
                    o.ParseStateValues = true;
                });
            });
    // .UseNLog();  // NLog: Setup NLog for Dependency injection
}
#pragma warning restore CS1591 // used for xml comment warning for swagger