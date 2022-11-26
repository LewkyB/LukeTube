using BenchmarkDotNet.Attributes;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.PollyPolicies;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace LukeTube.Benchmark;

[MemoryDiagnoser]
public class PushshiftRequestServiceBenchmarks
{
    private PushshiftRequestService _pushshiftRequestService;

    [GlobalSetup]
    public void GlobalSetup()
    {
        DbContextOptionsBuilder<PushshiftContext> dbOption = new DbContextOptionsBuilder<PushshiftContext>()
            .UseNpgsql("host=localhost;database=SubredditDb;username=postgres;password=postgres;")
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();

        var pushshiftRepository = new PushshiftRepository(new PushshiftContext(dbOption.Options), NullLogger<PushshiftRepository>.Instance);

        IServiceCollection services = new ServiceCollection();
        services.AddHttpClient<IPushshiftRequestService, PushshiftRequestService>("PushshiftRequestServiceClient",
                client => { client.BaseAddress = new Uri("https://api.pushshift.io/"); })
            .SetHandlerLifetime(TimeSpan.FromMinutes(2))
            .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
            .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());
        services.AddHttpClient<IPushshiftRequestService, PushshiftRequestService>("YoutubeExplodeClient");

        var httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

        _pushshiftRequestService = new PushshiftRequestService(
            NullLogger<PushshiftRequestService>.Instance,
            pushshiftRepository,
            httpClientFactory);
    }

    [Benchmark]
    public IReadOnlyList<PushshiftSearchOptions> GetSearchOptions_Small()
    {
        return _pushshiftRequestService.GetSearchOptions("www.youtube.com", 0, 500);
    }

    [Benchmark]
    public IReadOnlyList<PushshiftSearchOptions> GetSearchOptions_Medium()
    {
        return _pushshiftRequestService.GetSearchOptions("www.youtube.com", 10, 500);
    }

    [Benchmark]
    public IReadOnlyList<PushshiftSearchOptions> GetSearchOptions_Large()
    {
        return _pushshiftRequestService.GetSearchOptions("www.youtube.com", 100, 500);
    }
}