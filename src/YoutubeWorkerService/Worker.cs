using System.Threading.Channels;
using Elastic.Clients.Elasticsearch;
using LukeTubeLib.Diagnostics;
using LukeTubeLib.Models.HackerNews.Entities;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.Models.Pushshift.Entities;
using LukeTubeLib.Repositories;

namespace YoutubeWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IPushshiftRepository _pushshiftRepository;
    private readonly IHackerNewsRepository _hackerNewsRepository;
    private ChannelReader<IReadOnlyList<HackerNewsHit>> _hackerNewsChannelReader;
    private ChannelReader<IReadOnlyList<RedditComment>> _pushshiftChannelReader;

    public Worker(ILogger<Worker> logger, IPushshiftRepository pushshiftRepository, IHackerNewsRepository hackerNewsRepository, ChannelReader<IReadOnlyList<HackerNewsHit>> hackerNewsChannelReader, ChannelReader<IReadOnlyList<RedditComment>> pushshiftChannelReader)
    {
        _logger = logger;
        _pushshiftRepository = pushshiftRepository;
        _hackerNewsRepository = hackerNewsRepository;
        _hackerNewsChannelReader = hackerNewsChannelReader;
        _pushshiftChannelReader = pushshiftChannelReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var elasticSettings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
                .DefaultMappingFor<CommentViewModel>(index =>
                {
                    index.IndexName("pushshift-reddit-comment-index");
                    index.IdProperty(p => p.YoutubeId);
                })
                .DefaultMappingFor<HackerNewsHit>(index =>
                {
                    index.IndexName("hacker-news-hit-index");
                    index.IdProperty(p => p.HackerNewsHitId);
                });

        var elasticClient = new ElasticsearchClient(elasticSettings);

        while (!stoppingToken.IsCancellationRequested)
        {
            var redditComments = await _pushshiftChannelReader.ReadAsync(stoppingToken).ConfigureAwait(false);
            var hackerNewsHits = await _hackerNewsChannelReader.ReadAsync(stoppingToken).ConfigureAwait(false);

            if (redditComments.Count > 0)
            {
                PushshiftChannelCounterSource.Log.RemoveQueueCount(redditComments.Count);
                await _pushshiftRepository.SaveRedditComments(redditComments).ConfigureAwait(false);
                var commentViewModels = CommentViewModelHelper.MapEntityToViewModel(redditComments);
                await elasticClient.IndexManyAsync(commentViewModels, "pushshift-reddit-comment-index", stoppingToken);
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