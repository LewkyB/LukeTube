using System.Text.RegularExpressions;

namespace LukeTubeLib;

public static partial class FullYoutubeRegex
{
    // TODO: is the HTTP and WWW necessary?
    [GeneratedRegex(@"http(?:s?):\/\/(?:www\.)?youtu(?:be\.com\/watch\?v=|\.be\/)([\w\-\]*)(&(amp;)?[\w\?‌​=]*)?",
        RegexOptions.CultureInvariant,
        1000)]
    private static partial Regex YoutubeIdRegex();
    public static MatchCollection FindYoutubeIdMatches(string comment) => YoutubeIdRegex().Matches(comment);
}

public static partial class NormalYoutubeRegex
{
    // TODO: better name than normal?
    [GeneratedRegex(@"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)",
        RegexOptions.CultureInvariant,
        1000)]
    private static partial Regex YoutubeIdRegex();
    public static MatchCollection FindYoutubeIdMatches(string comment) => YoutubeIdRegex().Matches(comment);
}

public static partial class MinimalYoutubeRegex
{
    [GeneratedRegex(@"youtu\.be/(.*?)(?:\?|&|/|$)",
        RegexOptions.CultureInvariant,
        1000)]
    private static partial Regex YoutubeIdRegex();
    public static MatchCollection FindYoutubeIdMatches(string comment) => YoutubeIdRegex().Matches(comment);
}

public static partial class EmbeddedYoutubeRegex
{
    [GeneratedRegex(@"youtube\..+?/embed/(.*?)(?:\?|&|/|$)",
        RegexOptions.CultureInvariant,
        1000)]
    private static partial Regex YoutubeIdRegex();
    public static MatchCollection FindYoutubeIdMatches(string comment) => YoutubeIdRegex().Matches(comment);
}

public static partial class ShortsYoutubeRegex
{
    [GeneratedRegex(@"youtube\..+?/shorts/(.*?)(?:\?|&|/|$)",
        RegexOptions.CultureInvariant,
        1000)]
    private static partial Regex YoutubeIdRegex();
    public static MatchCollection FindYoutubeIdMatches(string comment) => YoutubeIdRegex().Matches(comment);
}