using System.Threading.RateLimiting;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using YoutubeExplode.Videos;

namespace LukeTubeWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IPushshiftRequestService _pushshiftRequestService;
    private readonly IPushshiftRepository _pushshiftRepository;

    private TokenBucketRateLimiterOptions rateLimitOptions = new()
    {
        TokenLimit = 8,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 3,
        ReplenishmentPeriod = TimeSpan.FromMilliseconds(1),
        TokensPerPeriod = 2,
        AutoReplenishment = true
    };

    public Worker(ILogger<Worker> logger, IPushshiftRequestService pushshiftRequestService, IPushshiftRepository pushshiftRepository)
    {
        _logger = logger;
        _pushshiftRequestService = pushshiftRequestService;
        _pushshiftRepository = pushshiftRepository;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        return DoWorkAsync(stoppingToken);
    }
    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        // var searchOptions = _pushshiftRequestService.GetSearchOptions("www.youtube.com/watch", 365, 100);
        var searchOptionsNoDates = _pushshiftRequestService.BuildSearchOptionsNoDates("www.youtube.com", 500);

        // using HttpClient client = new HttpClient(new ClientSideRateLimitedHandler(rateLimitOptions));
        using HttpClient client = new HttpClient(new ClientSideRateLimitedHandler(new TokenBucketRateLimiter(rateLimitOptions)));
        var videoHttpClient = new HttpClient();
        var videoClient = new VideoClient(videoHttpClient);
        client.BaseAddress =new Uri("https://api.pushshift.io/");

        int startDate = 0; // 0 is today, 1 is tomorrow, etc


        while (!cancellationToken.IsCancellationRequested)
        {
            var filledOutOptions = _pushshiftRequestService.AddBeforeAndAfter(searchOptionsNoDates, startDate);
            foreach (var searchOption in filledOutOptions)
            {
                // var redditComments = await _pushshiftRequestService.GetUniqueRedditComments(searchOption, client);
                var redditComments = await _pushshiftRequestService.GetUniqueRedditComments(searchOption, null);

                var newlySavedComments = await _pushshiftRepository.SaveRedditComments(redditComments);

                var videos = new List<Video>();
                foreach (var redditComment in newlySavedComments)
                {
                    try
                    {
                        var result =  await videoClient.GetAsync(redditComment.YoutubeLinkId, cancellationToken);
                        videos.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }

                await _pushshiftRepository.SaveVideos(videos);
            }

            startDate++;


            // var chunked = filledOutOptions.Chunk(10);
            // foreach (var chunk in chunked)
            // {
            //     foreach (var searchOption in chunk)
            //     {
            //         var tasks = new List<Task>();
            //         var semaphore = new SemaphoreSlim(10);
            //         await semaphore.WaitAsync(cancellationToken);
            //         try
            //         {
            //             // tasks.Add(GetPushshiftQueryResults<CommentResponse>("comment", searchOption));
            //             tasks.Add(_pushshiftRequestService.GetUniqueRedditComments(searchOption, client));
            //         }
            //         finally
            //         {
            //             semaphore.Release();
            //         }
            //         await Task.WhenAll(tasks);
            //     }
            // }
            //
            // startDate++;
        }
    }
}