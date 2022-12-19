using System.Threading.Channels;
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

    public static IServiceCollection AddPushshiftServicesForWorker(this IServiceCollection services)
    {
        services.AddSingleton<IPushshiftRequestService, PushshiftRequestService>();
        services.AddSingleton<IPushshiftRepository, PushshiftRepository>();

        services.AddDbContext<PushshiftContext>(options =>
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL_PUSHSHIFT"));
        }, ServiceLifetime.Singleton);

        // TODO: are singleton the best choice here? definitely not scoped
        services.AddHttpClient<IPushshiftRequestService, PushshiftRequestService>("PushshiftRequestServiceClient",
                client =>
                {
                    client.BaseAddress = new Uri(pushshiftBaseAddress);
                    client.Timeout = TimeSpan.FromSeconds(60);
                })
            .SetHandlerLifetime(TimeSpan.FromMinutes(2)) // TODO: if client timeout is 60s, does handler lifetime need to be 60 also?
            .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
            .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());

        return services;
    }

    public static IServiceCollection AddHackerNewsServicesForWorker(this IServiceCollection services)
    {
        // TODO: hn.algolia.com/api is rate limited to 10000/hr or 166/min, need to rework polly policy for it
        services.AddSingleton<IHackerNewsRequestService, HackerNewsRequestService>();
        services.AddSingleton<IHackerNewsRepository, HackerNewsRepository>();

        services.AddDbContext<HackerNewsContext>(options =>
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL_HACKERNEWS"));
        }, ServiceLifetime.Singleton);

        services.AddHttpClient<IHackerNewsRequestService, HackerNewsRequestService>("HackerNewsRequestServiceClient",
                client =>
                {
                    client.BaseAddress = new Uri(hackerNewsBaseAddress);
                    client.Timeout = TimeSpan.FromSeconds(60);
                })
            .SetHandlerLifetime(TimeSpan.FromMinutes(2)) // TODO: if client timeout is 60s, does handler lifetime need to be 60 also?
            .AddPolicyHandler(HackerNewsPolicies.GetWaitAndRetryPolicy())
            .AddPolicyHandler(HackerNewsPolicies.GetRateLimitPolicy());

        return services;
    }

    public static IServiceCollection AddUnboundedChannel<T>(this IServiceCollection services)
    {
        services.AddSingleton(Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            SingleReader = true,
        }));
        services.AddSingleton<ChannelReader<T>>(svc => svc.GetRequiredService<Channel<T>>().Reader);
        services.AddSingleton<ChannelWriter<T>>(svc => svc.GetRequiredService<Channel<T>>().Writer);

        return services;
    }
}