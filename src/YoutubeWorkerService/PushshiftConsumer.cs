using System.Threading.Channels;
using LukeTubeLib.Diagnostics;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.Models.Pushshift.Entities;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using MassTransit;
using YoutubeExplode.Videos;

namespace YoutubeWorkerService;

public class PushshiftConsumer : IConsumer<Batch<PushshiftMessage>>
{
    private readonly ILogger<PushshiftConsumer> _logger;
    private readonly HttpClient _httpClient;
    private readonly IPushshiftRepository _pushshiftRepository;
    private ChannelWriter<IReadOnlyList<RedditComment>> _channelWriter;

    public PushshiftConsumer(ILogger<PushshiftConsumer> logger, IPushshiftRepository pushshiftRepository, HttpClient httpClient, ChannelWriter<IReadOnlyList<RedditComment>> channelWriter)
    {
        _logger = logger;
        _pushshiftRepository = pushshiftRepository;
        _httpClient = httpClient;
        _channelWriter = channelWriter;
    }

    public async Task Consume(ConsumeContext<Batch<PushshiftMessage>> context)
    {
        var videoLimiter = new TaskLimiter(27, TimeSpan.FromSeconds(10));
        var videoClient = new VideoClient(_httpClient);

        var pushshiftMessages = context.Message.ToList();

        var redditComments = new List<RedditComment>();
        try
        {
            var videoTasks = pushshiftMessages
                .Select(consumeContext => videoLimiter.LimitAsync(() => videoClient.GetAsync(consumeContext.Message.YoutubeId)));

            var videos = (await Task.WhenAll(videoTasks).ConfigureAwait(false)).ToList();

            foreach (var message in pushshiftMessages)
            {
                var video = videos.FirstOrDefault(x => x.Id == message.Message.YoutubeId);

                if (video is null) continue;

                redditComments.Add(new RedditComment
                {
                    YoutubeId = message.Message.YoutubeId,
                    Permalink = message.Message.Permalink,
                    Score = message.Message.Score,
                    Subreddit = message.Message.Subreddit,
                    CreatedUTC = message.Message.CreatedUTC,
                    RetrievedUTC = message.Message.RetrievedUTC,
                    VideoModel = PushshiftVideoModelHelper.MapVideoEntity(video),
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        // await _pushshiftRepository.SaveRedditComments(redditComments);
        PushshiftChannelCounterSource.Log.AddQueueCount(redditComments.Count);
        await _channelWriter.WriteAsync(redditComments);
    }
}

public class PushshiftConsumerDefinition : ConsumerDefinition<PushshiftConsumer>
{
    public PushshiftConsumerDefinition()
    {
        EndpointName = "pushshift-consumer";
        ConcurrentMessageLimit = 8;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<PushshiftConsumer> consumerConfigurator)
    {
        consumerConfigurator.Options<BatchOptions>(options => options
            .SetMessageLimit(20)
            .SetTimeLimit(1000)
            .SetConcurrencyLimit(10));

        endpointConfigurator.UseMessageRetry(r => r.Intervals(100,200,500,800,1000));
        endpointConfigurator.UseInMemoryOutbox();
    }
}