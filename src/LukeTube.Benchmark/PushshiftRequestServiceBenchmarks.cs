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
            new HttpClient());
    }

    [Benchmark]
    public IList<PushshiftSearchOptions> AddBeforeAndAfter_100()
    {
        var searchOptions = _pushshiftRequestService.BuildSearchOptionsNoDates("www.youtube.com", 500);
        return _pushshiftRequestService.AddBeforeAndAfter(searchOptions, 100);
    }

    [Benchmark]
    public IList<PushshiftSearchOptions> AddBeforeAndAfter_1000()
    {
        var searchOptions = _pushshiftRequestService.BuildSearchOptionsNoDates("www.youtube.com", 500);
        return _pushshiftRequestService.AddBeforeAndAfter(searchOptions, 1000);
    }
}