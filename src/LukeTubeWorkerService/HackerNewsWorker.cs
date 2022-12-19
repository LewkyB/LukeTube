using System.Threading.Channels;
using LukeTubeLib.Models.HackerNews;
using LukeTubeLib.Services;

namespace LukeTubeWorkerService;

public class HackerNewsWorker : BackgroundService
{
    private readonly ILogger<HackerNewsWorker> _logger;
    private readonly IHackerNewsRequestService _hackerNewsRequestService;
    private ChannelWriter<IReadOnlyList<HackerNewsMessage>> _channelWriter;

    public HackerNewsWorker(
        ILogger<HackerNewsWorker> logger,
        IHackerNewsRequestService hackerNewsRequestService,
        ChannelWriter<IReadOnlyList<HackerNewsMessage>> channelWriter)
    {
        _logger = logger;
        _hackerNewsRequestService = hackerNewsRequestService;
        _channelWriter = channelWriter;
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

        while (!cancellationToken.IsCancellationRequested)
        {
            var filledOutHackerNewsOptions = Enumerable.Range(startDate, startDate+3)
                .SelectMany(day => _hackerNewsRequestService.AddBeforeAndAfter(new HackerNewsSearchOptions
                {
                    HitsPerPage = 1000,
                    After = null,
                    Before = null,
                    Query = "www.youtube.com",
                    Tags = "comment"
                }, day));

            var tasks = filledOutHackerNewsOptions
                .Select(searchOption =>
                    limiter.LimitAsync(() => _hackerNewsRequestService.GetHackerNewsHits(searchOption, cancellationToken)));

            Task[] processingTasks = tasks.Select(async task =>
            {
                var result = await task.ConfigureAwait(false);
                await _channelWriter.WriteAsync(result, cancellationToken);
            }).ToArray();

            await Task.WhenAll(processingTasks).ConfigureAwait(false);

            // increment by 3 because above we're batching by 3
            startDate += 3;
        }
    }
}