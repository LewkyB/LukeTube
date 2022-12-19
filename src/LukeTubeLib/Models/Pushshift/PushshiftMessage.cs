using LukeTubeLib.Models.Pushshift.Entities;
using YoutubeExplode.Videos;

namespace LukeTubeLib.Models.Pushshift;

public class PushshiftMessage
{
    public string Subreddit { get; init; }

    public string YoutubeId { get; set; }

    public int Score { get; set; }

    public DateTime CreatedUTC { get; set; }

    public DateTime RetrievedUTC { get; set; }

    public string Permalink { get; set; }
}

public static class PushshiftMessageExtensions
{
    public static RedditComment ToRedditComment(this PushshiftMessage message, Video video)
    {
        return new RedditComment
        {
            YoutubeId = message.YoutubeId,
            Permalink = message.Permalink,
            Score = message.Score,
            Subreddit = message.Subreddit,
            CreatedUTC = message.CreatedUTC,
            RetrievedUTC = message.RetrievedUTC,
            VideoModel = PushshiftVideoModelHelper.MapVideoEntity(video),
        };
    }
}