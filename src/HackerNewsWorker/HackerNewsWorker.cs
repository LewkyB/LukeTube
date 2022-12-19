using LukeTubeLib.Models.HackerNews;
using LukeTubeLib.Models.HackerNews.Entities;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using YoutubeExplode.Videos;

namespace HackerNewsWorker;

public class HackerNewsWorker : BackgroundService
{
    private readonly ILogger<HackerNewsWorker> _logger;
    private readonly IHackerNewsRequestService _hackerNewsRequestService;
    private readonly IHackerNewsRepository _hackerNewsRepository;
    private readonly HttpClient _httpClient;

    public HackerNewsWorker(
        ILogger<HackerNewsWorker> logger,
        IHackerNewsRequestService hackerNewsRequestService,
        IHackerNewsRepository hackerNewsRepository, HttpClient httpClient)
    {
        _logger = logger;
        _hackerNewsRequestService = hackerNewsRequestService;
        _hackerNewsRepository = hackerNewsRepository;
        _httpClient = httpClient;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        return DoWorkAsync(stoppingToken);
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        int startDate = 0;

        // hn.algolia.com is limited to 166.67 requests per min
        var limiter = new TaskLimiter(27, TimeSpan.FromSeconds(10));

        var videoClient = new VideoClient(_httpClient);

        while (!cancellationToken.IsCancellationRequested)
        {
            var filledoutHackerNewsOptions = _hackerNewsRequestService.AddBeforeAndAfter(new HackerNewsSearchOptions
            {
                HitsPerPage = 1000,
                After = null,
                Before = null,
                Query = "www.youtube.com",
                Tags = "comment"
            }, startDate);

            var hackerNewsHits = new List<HackerNewsHit>();

            var tasks = filledoutHackerNewsOptions
                .Select(searchOption =>
                    limiter.LimitAsync(() => _hackerNewsRequestService.GetHackerNewsHits(searchOption, cancellationToken)));

            Task[] processingTasks = tasks.Select(async task =>
            {
                var result = await task.ConfigureAwait(false);
                // hackerNewsHits.AddRange(result);
            }).ToArray();

            await Task.WhenAll(processingTasks).ConfigureAwait(false);

            Video[] videoResults = null;
            try
            {
                var videoTasks = hackerNewsHits
                    .Select(hit => limiter.LimitAsync(() => videoClient.GetAsync(hit.YoutubeId, cancellationToken)));

                videoResults = await Task.WhenAll(videoTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            if (videoResults is not null)
            {
                foreach (var result in videoResults)
                {
                    var hitWithoutVideo = hackerNewsHits.FirstOrDefault(x => x.YoutubeId == result.Id);

                    if (hitWithoutVideo is not null) hitWithoutVideo.VideoModel = HackerNewsVideoModelHelper.MapVideoEntity(result);
                }

                var hitsToSave = hackerNewsHits.Where(x => x.VideoModel is not null).ToList();
                await _hackerNewsRepository.SaveHackerNewsHits(hitsToSave).ConfigureAwait(false);
            }

            startDate++;
        }
    }
}