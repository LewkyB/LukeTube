namespace LukeTubeLib.Services;

public static class YoutubeUtilities
{
    public static IReadOnlyList<string> FindYoutubeId(string commentBody)
    {
        if (string.IsNullOrEmpty(commentBody)) return new List<string>();

        if (IsValidYoutubeId(commentBody)) return new []{ commentBody };

        // TODO: this is definitely in the hot path, is LINQ the best choice?
        // TODO: wow there has got to be another way to combine all this regex?
        // TODO: is it overkill to combine all of these or should I add some conditional logic?
        var shortsMatches = ShortsYoutubeRegex.FindYoutubeIdMatches(commentBody);
        var fullMatches = FullYoutubeRegex.FindYoutubeIdMatches(commentBody);
        var normalMatches = NormalYoutubeRegex.FindYoutubeIdMatches(commentBody);
        var minimalMatches = MinimalYoutubeRegex.FindYoutubeIdMatches(commentBody);
        var embeddedMatches = EmbeddedYoutubeRegex.FindYoutubeIdMatches(commentBody);
        var linkMatches = shortsMatches
            .Union(fullMatches)
            .Union(normalMatches)
            .Union(minimalMatches)
            .Union(minimalMatches)
            .Union(embeddedMatches);

        var youtubeIds = new List<string>();
        foreach (var match in linkMatches.DistinctBy(x => x.Groups[1].Value))
        {
            if (IsValidYoutubeId(match.Groups[1].Value)) youtubeIds.Add(match.Groups[1].Value);
        }

        return youtubeIds;
    }

    public static bool IsValidYoutubeId(string videoId) =>
        videoId.Length == 11 &&
        videoId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    public static string ArgsToString(IEnumerable<string> args)
        => args.Aggregate((x, y) => $"{x}&{y}");
}