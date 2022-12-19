using LukeTubeLib;
using LukeTubeLib.Models.HackerNews.Entities;
using LukeTubeLib.Models.Pushshift.Entities;
using MassTransit;
using YoutubeWorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.AddOpenTelemetryLogging();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();

        services.AddHackerNewsServicesForWorker();
        services.AddSingleton<HackerNewsConsumer>();
        services.AddHttpClient<HackerNewsConsumer>();

        services.AddPushshiftServicesForWorker();
        services.AddSingleton<PushshiftConsumer>();
        services.AddHttpClient<PushshiftConsumer>();

        services.AddUnboundedChannel<IReadOnlyList<HackerNewsHit>>();
        services.AddUnboundedChannel<IReadOnlyList<RedditComment>>();

        services.AddOpenTelemetryTracing();
        services.AddOpenTelemetryMetrics();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<HackerNewsConsumer>(typeof(HackerNewsConsumerDefinition))
                .Endpoint(e =>
                {
                    e.Name = "hacker-news-consumer";
                    e.Temporary = false;
                    e.ConcurrentMessageLimit = 8;
                    e.PrefetchCount = 16;
                    e.InstanceId = "hacker-news-unique";
                });

            x.AddConsumer<PushshiftConsumer>(typeof(PushshiftConsumerDefinition))
                .Endpoint(e =>
                {
                    e.Name = "pushshift-consumer";
                    e.Temporary = false;
                    e.ConcurrentMessageLimit = 8;
                    e.PrefetchCount = 16;
                    e.InstanceId = "pushshift-unique";
                });

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) => cfg.ConfigureEndpoints(context));
        });
    })
    .Build();

host.Run();