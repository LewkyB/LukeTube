namespace LukeTubeLib.Models.Pushshift;

public record SubredditWithCount
{
    public string? Subreddit { get; init; }
    public int? Count { get; init; }
}