using System.Threading.Channels;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.Services;

namespace LukeTubeWorkerService;

public class PushshiftWorker : BackgroundService
{
    private readonly ILogger<PushshiftWorker> _logger;
    private readonly IPushshiftRequestService _pushshiftRequestService;
    private ChannelWriter<IReadOnlyList<PushshiftMessage>> _channelWriter;

    public PushshiftWorker(
        ILogger<PushshiftWorker> logger,
        IPushshiftRequestService pushshiftRequestService,
        ChannelWriter<IReadOnlyList<PushshiftMessage>> channelWriter)
    {
        _logger = logger;
        _pushshiftRequestService = pushshiftRequestService;
        _channelWriter = channelWriter;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        return DoWorkAsync(stoppingToken);
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        var pushshiftSearchOptionsNoDates =
            _pushshiftRequestService.BuildSearchOptionsNoDates("www.youtube.com", 500);
        int startDate = 0;
        var semaphoreSlim = new SemaphoreSlim(1);

        while (!cancellationToken.IsCancellationRequested)
        {
            var allSearchOptions = Enumerable.Range(startDate, startDate+5)
                .SelectMany(day => _pushshiftRequestService.AddBeforeAndAfter(pushshiftSearchOptionsNoDates, day))
                .Chunk(60);
            var tasks = new List<Task>();

            // start all the tasks in intervals with the last task starting at
            // 55 seconds, leaving 5 seconds for the last request to finish
            var interval = 55000 / 60;

            foreach (var chunk in allSearchOptions)
            {
                foreach (var searchOption in chunk)
                {
                    await semaphoreSlim.WaitAsync(cancellationToken);
                    tasks.Add(Task.Factory.StartNew(() => _pushshiftRequestService.GetRedditComments(searchOption, cancellationToken), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                        .Unwrap()
                        .ContinueWith(async result =>
                        {
                            var pushshiftMessages = await result.ConfigureAwait(false);
                            if (pushshiftMessages.Count > 0) await _channelWriter.WriteAsync(pushshiftMessages, cancellationToken).ConfigureAwait(false);
                        }, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current));

                    semaphoreSlim.Release();

                    await Task.Delay(interval, cancellationToken);
                }
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            startDate += 5;
        }
    }
}