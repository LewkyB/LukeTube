using System.Threading.Channels;
using LukeTubeLib.Diagnostics;
using LukeTubeLib.Models.HackerNews;
using LukeTubeLib.Models.HackerNews.Entities;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using MassTransit;
using YoutubeExplode.Videos;

namespace YoutubeWorkerService;

public class HackerNewsConsumer : IConsumer<Batch<HackerNewsMessage>>
{
    private readonly ILogger<HackerNewsConsumer> _logger;
    private readonly HttpClient _httpClient;
    private readonly IHackerNewsRepository _hackerNewsRepository;
    private ChannelWriter<IReadOnlyList<HackerNewsHit>> _channelWriter;

    public HackerNewsConsumer(
        ILogger<HackerNewsConsumer> logger,
        IHackerNewsRepository hackerNewsRepository,
        HttpClient httpClient,
        ChannelWriter<IReadOnlyList<HackerNewsHit>> channelWriter)
    {
        _logger = logger;
        _hackerNewsRepository = hackerNewsRepository;
        _httpClient = httpClient;
        _channelWriter = channelWriter;
    }

    public async Task Consume(ConsumeContext<Batch<HackerNewsMessage>> context)
    {
        var videoLimiter = new TaskLimiter(27, TimeSpan.FromSeconds(10));
        var videoClient = new VideoClient(_httpClient);

        var hackerNewsMessages = context.Message.ToList();

        var hackerNewsHits = new List<HackerNewsHit>();
        try
        {
            var videoTasks = hackerNewsMessages
                .Select(consumeContext => videoLimiter.LimitAsync(() => videoClient.GetAsync(consumeContext.Message.YoutubeId)));

            var videos = (await Task.WhenAll(videoTasks).ConfigureAwait(false)).ToList();

            foreach (var message in hackerNewsMessages)
            {
                var video = videos.FirstOrDefault(x => x.Id == message.Message.YoutubeId);

                if (video is null) continue;

                hackerNewsHits.Add(new HackerNewsHit
                {
                    YoutubeId = message.Message.YoutubeId,
                    Author = message.Message.Author,
                    Points = message.Message.Points,
                    Url = message.Message.Url,
                    VideoModel = HackerNewsVideoModelHelper.MapVideoEntity(video),
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        // await _hackerNewsRepository.SaveHackerNewsHits(hackerNewsHits);

        HackerNewsChannelCounterSource.Log.AddQueueCount(hackerNewsHits.Count);
        await _channelWriter.WriteAsync(hackerNewsHits);
    }
}

public class HackerNewsConsumerDefinition : ConsumerDefinition<HackerNewsConsumer>
{
    public HackerNewsConsumerDefinition()
    {
        EndpointName = "hacker-news-consumer";
        ConcurrentMessageLimit = 8;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<HackerNewsConsumer> consumerConfigurator)
    {
        consumerConfigurator.Options<BatchOptions>(options => options
            .SetMessageLimit(20)
            .SetTimeLimit(1000)
            .SetConcurrencyLimit(10));

        endpointConfigurator.UseMessageRetry(r => r.Intervals(100,200,500,800,1000));
        endpointConfigurator.UseInMemoryOutbox();
    }
}