using System.Diagnostics;
using System.Net;
using System.Threading.Channels;
using LukeTubeLib.Diagnostics;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.Models.Pushshift.Entities;
using LukeTubeLib.Services;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;

namespace LukeTubeWorkerService;

public class PushshiftYoutubeWorker : BackgroundService
{
    private readonly ILogger<PushshiftYoutubeWorker> _logger;
    private ChannelReader<IReadOnlyList<PushshiftMessage>> _channelReader;
    private ChannelWriter<RedditComment> _channelWriter;
    private readonly HttpClient _httpClient;

    public PushshiftYoutubeWorker(
        ILogger<PushshiftYoutubeWorker> logger,
        ChannelReader<IReadOnlyList<PushshiftMessage>> channelReader,
        ChannelWriter<RedditComment> channelWriter,
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

                        PushshiftChannelCounterSource.Log.AddQueueCount(1);

                        await _channelWriter.WriteAsync(message.ToRedditComment(video), stoppingToken).ConfigureAwait(false);
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