using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using luke_site_mvc.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using StackExchange.Profiling.Storage;

namespace luke_site_mvc
{
#pragma warning disable CS1591 // used for disabling xml comment warning for swagger
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");

                CreateHostBuilder(args).Build().Run();
                //var host = CreateHostBuilder(args).Build();

                //using (var scope = host.Services.CreateScope())
                //{
                //    //var services = scope.ServiceProvider;
                //    //var seeder = host.Services.GetRequiredService<ISeedDataRepository>();

                //    var seeder = scope.ServiceProvider.GetRequiredService<SeedDataRepository>();
                //    //var seeder = scope.ServiceProvider.GetRequiredService<ISeedDataRepository>();
                //    //ISeedDataRepository seedDataRepository = scope.ServiceProvider.GetRequiredService<ISeedDataRepository>();
                //    //SeedDataRepository seedDataRepository = new SeedDataRepository();
                //    await seeder.Initialize();
                //    //await seedDataRepository.Initialize();
                //}
                //var seeder = host.Services.CreateScope .GetRequiredService<ISeedDataRepository>();
                //await seeder.Initialize();
                //await RunSeedingAsync(host);

                //host.Run();
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

        private static async Task RunSeedingAsync(IHost host)
        {
            var scopeFactory = host.Services.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetService<SeedDataRepository>();
                await seeder.Initialize();
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
                })
                .UseNLog();  // NLog: Setup NLog for Dependency injection
    }
#pragma warning restore CS1591 // used for xml comment warning for swagger
}
