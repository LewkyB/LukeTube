using System.Threading.RateLimiting;
using LukeTubeLib.PollyPolicies;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LukeTubeLib;

public static class DependencyInjection
{
    private const string pushshiftBaseAddress = "https://api.pushshift.io/";

    // private static TokenBucketRateLimiterOptions rateLimitOptions = new()
    // {
    //     TokenLimit = 8,
    //     QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    //     QueueLimit = 3,
    //     ReplenishmentPeriod = TimeSpan.FromMilliseconds(1),
    //     TokensPerPeriod = 2,
    //     AutoReplenishment = true
    // };

    public static IServiceCollection AddPushshiftServicesForWorker(this IServiceCollection services)
    {
        // TODO: are singleton the best choice here? definitely not scoped
        services.AddHttpClient<IPushshiftRequestService, PushshiftRequestService>("PushshiftRequestServiceClient", client =>
            {
                client.BaseAddress = new Uri(pushshiftBaseAddress);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(2))
            .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
            .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());

        services.AddSingleton<IPushshiftRequestService, PushshiftRequestService>();
        services.AddSingleton<IPushshiftRepository, PushshiftRepository>();
        services.AddDbContext<PushshiftContext>(options =>
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL"));
        }, ServiceLifetime.Singleton);

    return services;
}
}