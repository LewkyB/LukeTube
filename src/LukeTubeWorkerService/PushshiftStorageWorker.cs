using System.Threading.Channels;
using LukeTubeLib.Diagnostics;
using LukeTubeLib.Models.HackerNews.Entities;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.Models.Pushshift.Entities;
using LukeTubeLib.Repositories;
using Nest;

namespace LukeTubeWorkerService;

public class PushshiftStorageWorker : BackgroundService
{
    private readonly ILogger<PushshiftStorageWorker> _logger;
    private readonly IPushshiftRepository _pushshiftRepository;
    private ChannelReader<RedditComment> _pushshiftChannelReader;

    public PushshiftStorageWorker(
        ILogger<PushshiftStorageWorker> logger,
        IPushshiftRepository pushshiftRepository,
        ChannelReader<RedditComment> pushshiftChannelReader)
    {
        _logger = logger;
        _pushshiftRepository = pushshiftRepository;
        _pushshiftChannelReader = pushshiftChannelReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var elasticSettings = new ConnectionSettings(new Uri("http://localhost:9200"));
                // .DefaultMappingFor<CommentViewModel>(index =>
                // {
                //     index.IndexName("pushshift-reddit-comment-index");
                //     index.IdProperty(p => p.YoutubeId);
                // })
                // .DefaultMappingFor<HackerNewsHit>(index =>
                // {
                //     index.IndexName("hacker-news-hit-index");
                //     index.IdProperty(p => p.HackerNewsHitId);
                // });

        var elasticClient = new ElasticClient(elasticSettings);

        var createIndexResponse = await elasticClient.Indices.CreateAsync("pushshift-reddit-comment-index", c => c
            .Map<CommentViewModel>(m => m
                .AutoMap<CommentViewModel>()
            ), stoppingToken);


        while (!stoppingToken.IsCancellationRequested)
        {
            var redditComments = new List<RedditComment>();

            while (redditComments.Count < 20)
            {
                redditComments.Add(await _pushshiftChannelReader.ReadAsync(stoppingToken));
            }

            if (redditComments.Count > 0)
            {
                PushshiftChannelCounterSource.Log.RemoveQueueCount(redditComments.Count);
                await _pushshiftRepository.SaveRedditComments(redditComments).ConfigureAwait(false);
                var commentViewModels = CommentViewModelHelper.MapEntityToViewModel(redditComments);
                await elasticClient.IndexManyAsync(commentViewModels, "pushshift-reddit-comment-index", stoppingToken);
            }
        }
    }
}