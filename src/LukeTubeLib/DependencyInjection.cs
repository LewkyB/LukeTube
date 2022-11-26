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
    private const string hackerNewsBaseAddress = "https://hn.algolia.com/api/v1/";

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
        services.AddSingleton<IPushshiftRequestService, PushshiftRequestService>();
        services.AddSingleton<IPushshiftRepository, PushshiftRepository>();
        services.AddDbContext<PushshiftContext>(options =>
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL"));
        }, ServiceLifetime.Singleton);

        // TODO: are singleton the best choice here? definitely not scoped
        services.AddHttpClient<IPushshiftRequestService, PushshiftRequestService>("PushshiftRequestServiceClient", client =>
            {
                client.BaseAddress = new Uri(pushshiftBaseAddress);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(2))
            .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
            .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());
        services.AddHttpClient<IPushshiftRequestService, PushshiftRequestService>("YoutubeExplodeClient");

        // TODO: hn.algolia.com/api is rate limited to 10000/hr or 166/min, need to rework polly policy for it
        services.AddSingleton<IHackerNewsRequestService, HackerNewsRequestService>();
        services.AddSingleton<IHackerNewsRepository, HackerNewsRepository>();
        services.AddDbContext<HackerNewsContext>(options =>
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL_HACKERNEWS"));
        }, ServiceLifetime.Singleton);
        services.AddHttpClient<IHackerNewsRequestService, HackerNewsRequestService>("HackerNewsRequestServiceClient", client =>
            {
                client.BaseAddress = new Uri(hackerNewsBaseAddress);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(2))
            .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
            .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());
        services.AddHttpClient<IHackerNewsRequestService, HackerNewsRequestService>("YoutubeExplodeClient");

        return services;
    }
}