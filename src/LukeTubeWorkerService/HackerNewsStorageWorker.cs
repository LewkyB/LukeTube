using System.Threading.Channels;
using LukeTubeLib.Diagnostics;
using LukeTubeLib.Models.HackerNews.Entities;
using LukeTubeLib.Repositories;
using Nest;

namespace LukeTubeWorkerService;

public class HackerNewsStorageWorker : BackgroundService
{
    private readonly ILogger<HackerNewsStorageWorker> _logger;
    private readonly IHackerNewsRepository _hackerNewsRepository;
    private ChannelReader<HackerNewsHit> _hackerNewsChannelReader;

    public HackerNewsStorageWorker(
        ILogger<HackerNewsStorageWorker> logger,
        IHackerNewsRepository hackerNewsRepository,
        ChannelReader<HackerNewsHit> hackerNewsChannelReader)
    {
        _logger = logger;
        _hackerNewsRepository = hackerNewsRepository;
        _hackerNewsChannelReader = hackerNewsChannelReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // var elasticSettings = new ConnectionSettings(new Uri("http://localhost:9200"));

        // var elasticClient = new ElasticClient(elasticSettings);
        //
        // var createIndexResponse = await elasticClient.Indices.CreateAsync("pushshift-reddit-comment-index", c => c
        //     .Map<CommentViewModel>(m => m
        //         .AutoMap<CommentViewModel>()
        //     ), stoppingToken);


        while (!stoppingToken.IsCancellationRequested)
        {
            var hackerNewsHits = new List<HackerNewsHit>();

            while (hackerNewsHits.Count < 20)
            {
                hackerNewsHits.Add(await _hackerNewsChannelReader.ReadAsync(stoppingToken));
            }

            if (hackerNewsHits.Count > 0)
            {
                HackerNewsChannelCounterSource.Log.RemoveQueueCount(hackerNewsHits.Count);
                await _hackerNewsRepository.SaveHackerNewsHits(hackerNewsHits).ConfigureAwait(false);
                // await elasticClient.IndexManyAsync(hackerNewsHits, "hacker-news-hit-index", stoppingToken);
            }
        }
    }
}