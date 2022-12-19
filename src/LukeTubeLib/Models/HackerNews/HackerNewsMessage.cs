using LukeTubeLib.Models.HackerNews.Entities;
using YoutubeExplode.Videos;

namespace LukeTubeLib.Models.HackerNews;

public record HackerNewsMessage
{
    public string Author { get; init; }
    public string Url { get; init; }
    public int? Points { get; init; }
    public string YoutubeId { get; init; }
}

public static class HackerNewsMessageExtensions
{
    public static HackerNewsHit ToHackerNewsHit(this HackerNewsMessage message, Video video)
    {
        return new HackerNewsHit
        {
            YoutubeId = message.YoutubeId,
            Author = message.Author,
            Points = message.Points,
            Url = message.Url,
            VideoModel = HackerNewsVideoModelHelper.MapVideoEntity(video),
        };
    }
}