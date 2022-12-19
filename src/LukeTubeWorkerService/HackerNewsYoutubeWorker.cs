using System.Diagnostics;
using System.Threading.Channels;
using LukeTubeLib.Diagnostics;
using LukeTubeLib.Models.HackerNews;
using LukeTubeLib.Models.HackerNews.Entities;
using LukeTubeLib.Services;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;

namespace LukeTubeWorkerService;

public class HackerNewsYoutubeWorker : BackgroundService
{
    private readonly ILogger<HackerNewsYoutubeWorker> _logger;
    private ChannelReader<IReadOnlyList<HackerNewsMessage>> _channelReader;
    private ChannelWriter<HackerNewsHit> _channelWriter;
    private readonly HttpClient _httpClient;

    public HackerNewsYoutubeWorker(
        ILogger<HackerNewsYoutubeWorker> logger,
        ChannelReader<IReadOnlyList<HackerNewsMessage>> channelReader,
        ChannelWriter<HackerNewsHit> channelWriter,
        HttpClient httpClient)
    {
        _logger = logger;
        _channelReader = channelReader;
        _channelWriter = channelWriter;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var videoLimiter = new TaskLimiter(27, TimeSpan.FromSeconds(10));
        var videoClient = new VideoClient(_httpClient);

        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await _channelReader.ReadAsync(stoppingToken);

            try
            {
                var videoTasks = messages
                    .Select(message => videoLimiter.LimitAsync(() => videoClient.GetAsync(message.YoutubeId, stoppingToken)));

                Task[] processingTasks = videoTasks.Select(async videoTask =>
                {
                    Video video = null;
                    try
                    {
                        video = await videoTask;
                    }
                    catch (VideoUnavailableException) {}
                    catch (Exception ex) { _logger.LogInformation(ex, ex.Message); }

                    if (video is not null)
                    {
                        // match the video up with the message, should never be null
                        var message = messages.FirstOrDefault(x => x.YoutubeId == video.Id) ?? throw new UnreachableException();

                        HackerNewsChannelCounterSource.Log.AddQueueCount(1);

                        await _channelWriter.WriteAsync(message.ToHackerNewsHit(video), stoppingToken).ConfigureAwait(false);
                    }
                }).ToArray();

                await Task.WhenAll(processingTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}