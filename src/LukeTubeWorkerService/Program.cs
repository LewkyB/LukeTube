using LukeTubeLib;
using LukeTubeLib.Models.HackerNews;
using LukeTubeLib.Models.HackerNews.Entities;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.Models.Pushshift.Entities;
using LukeTubeWorkerService;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.AddOpenTelemetryLogging();
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        logging.SetMinimumLevel(LogLevel.Error);
    })
    .ConfigureServices(services =>
    {
        // Pushshift
        services.AddPushshiftServicesForWorker();
        services.AddHostedService<PushshiftWorker>();

        services.AddHostedService<PushshiftYoutubeWorker>();
        services.AddHttpClient<PushshiftYoutubeWorker>();

        services.AddHostedService<PushshiftStorageWorker>();
        services.AddUnboundedChannel<RedditComment>();
        services.AddUnboundedChannel<IReadOnlyList<PushshiftMessage>>();

        // Hacker News
        services.AddHackerNewsServicesForWorker();
        services.AddHostedService<HackerNewsWorker>();

        services.AddHostedService<HackerNewsYoutubeWorker>();
        services.AddHttpClient<HackerNewsYoutubeWorker>();

        services.AddHostedService<HackerNewsStorageWorker>();
        services.AddUnboundedChannel<HackerNewsHit>();
        services.AddUnboundedChannel<IReadOnlyList<HackerNewsMessage>>();

        services.AddOpenTelemetry();

        // TODO: better way to reduce the amount of logs from http client factory?
        services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
    })
    .Build();

host.Run();