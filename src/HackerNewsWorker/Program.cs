using LukeTubeLib;
using LukeTubeLib.PollyPolicies;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<HackerNewsWorker.HackerNewsWorker>();
        services.AddHackerNewsServicesForWorker();
        services.AddHttpClient<HackerNewsWorker.HackerNewsWorker>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(2))
            .AddPolicyHandler(HackerNewsPolicies.GetWaitAndRetryPolicy())
            .AddPolicyHandler(HackerNewsPolicies.GetRateLimitPolicy());
    })
    .Build();

host.Run();