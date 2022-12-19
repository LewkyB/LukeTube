using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace LukeTubeLib.Models.HackerNews;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
// public record HackerNewsAuthor(
//     [property: JsonPropertyName("value")] string Value,
//     [property: JsonPropertyName("matchLevel")] string MatchLevel,
//     [property: JsonPropertyName("matchedWords")] IReadOnlyList<object> MatchedWords
// );
//
// public record HighlightResult(
//     [property: JsonPropertyName("title")] Title Title,
//     [property: JsonPropertyName("author")] Author Author,
//     [property: JsonPropertyName("story_text")] StoryText StoryText
// );
//
// public record Hit(
//     [property: JsonPropertyName("created_at")] DateTime CreatedAt,
//     [property: JsonPropertyName("title")] string Title,
//     [property: JsonPropertyName("url")] object Url,
//     [property: JsonPropertyName("author")] string Author,
//     [property: JsonPropertyName("points")] int? Points,
//     [property: JsonPropertyName("story_text")] string StoryText,
//     [property: JsonPropertyName("comment_text"), JsonConverter(typeof(HtmlDecodeConverter))] string CommentText,
//     [property: JsonPropertyName("num_comments")] int? NumComments,
//     [property: JsonPropertyName("story_id")] object StoryId,
//     [property: JsonPropertyName("story_title")] object StoryTitle,
//     [property: JsonPropertyName("story_url")] object StoryUrl,
//     [property: JsonPropertyName("parent_id")] object ParentId,
//     [property: JsonPropertyName("created_at_i")] int CreatedAtI,
//     [property: JsonPropertyName("_tags")] IReadOnlyList<string> Tags,
//     [property: JsonPropertyName("objectID")] string ObjectId,
//     [property: JsonPropertyName("_highlightResult")] HighlightResult HighlightResult
// );
//
// public record Root(
//     [property: JsonPropertyName("hits")] IReadOnlyList<Hit> Hits,
//     [property: JsonPropertyName("nbHits")] int nbHits,
//     [property: JsonPropertyName("page")] int page,
//     [property: JsonPropertyName("nbPages")] int nbPages,
//     [property: JsonPropertyName("hitsPerPage")] int hitsPerPage,
//     [property: JsonPropertyName("exhaustiveNbHits")] bool exhaustiveNbHits,
//     [property: JsonPropertyName("exhaustiveTypo")] bool exhaustiveTypo,
//     [property: JsonPropertyName("exhaustive")] Exhaustive exhaustive,
//     [property: JsonPropertyName("query")] string query,
//     [property: JsonPropertyName("params")] string @params,
//     [property: JsonPropertyName("processingTimeMS")] int processingTimeMS,
//     [property: JsonPropertyName("processingTimingsMS")] ProcessingTimingsMS processingTimingsMS
// );
// public record ProcessingTimingsMS(
//     [property: JsonPropertyName("afterFetch")] AfterFetch afterFetch,
//     [property: JsonPropertyName("fetch")] Fetch fetch,
//     [property: JsonPropertyName("total")] int total
// );
// public record Exhaustive(
//     [property: JsonPropertyName("nbHits")] bool nbHits,
//     [property: JsonPropertyName("typo")] bool typo
// );
//
// public record Fetch(
//     [property: JsonPropertyName("total")] int total
// );
// public record AfterFetch(
//     [property: JsonPropertyName("total")] int total
// );
// public record StoryText(
//     [property: JsonPropertyName("value")] string Value,
//     [property: JsonPropertyName("matchLevel")] string MatchLevel,
//     [property: JsonPropertyName("fullyHighlighted")] bool FullyHighlighted,
//     [property: JsonPropertyName("matchedWords")] IReadOnlyList<string> MatchedWords
// );
//
// public record Title(
//     [property: JsonPropertyName("value")] string Value,
//     [property: JsonPropertyName("matchLevel")] string MatchLevel,
//     [property: JsonPropertyName("matchedWords")] IReadOnlyList<object> MatchedWords
// );
//
// public record HackerNewsResponse
// (
// // public class HackerNewsResponse
// // {
//     // [JsonPropertyName("hits")]
//     // public IReadOnlyList<Hit>? Hits { get; set; }
//     [property: JsonPropertyName("hits")] IReadOnlyList<Hit> Hits,
//     [property: JsonPropertyName("nbHits")] int nbHits,
//     [property: JsonPropertyName("page")] int page,
//     [property: JsonPropertyName("nbPages")]
//     int nbPages,
//     [property: JsonPropertyName("hitsPerPage")]
//     int hitsPerPage,
//     [property: JsonPropertyName("exhaustiveNbHits")]
//     bool exhaustiveNbHits,
//     [property: JsonPropertyName("exhaustiveTypo")]
//     bool exhaustiveTypo,
//     [property: JsonPropertyName("exhaustive")]
//     Exhaustive exhaustive,
//     [property: JsonPropertyName("query")] string query,
//     [property: JsonPropertyName("params")] string @params,
//     [property: JsonPropertyName("processingTimeMS")]
//     int processingTimeMS,
//     [property: JsonPropertyName("processingTimingsMS")]
//     ProcessingTimingsMS processingTimingsMS
// );
// // }

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class AfterFetch
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class HackerNewsAuthor
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("matchLevel")]
    public string MatchLevel { get; set; }

    [JsonPropertyName("matchedWords")]
    public List<object> MatchedWords { get; set; } = new List<object>();
}

public class CommentText
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("matchLevel")]
    public string MatchLevel { get; set; }

    [JsonPropertyName("fullyHighlighted")]
    public bool FullyHighlighted { get; set; }

    [JsonPropertyName("matchedWords")]
    public List<string> MatchedWords { get; set; } = new List<string>();
}

public class Exhaustive
{
    [JsonPropertyName("nbHits")]
    public bool NbHits { get; set; }

    [JsonPropertyName("typo")]
    public bool Typo { get; set; }
}

public class Fetch
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class HighlightResult
{
    [JsonPropertyName("author")]
    public HackerNewsAuthor Author { get; set; }

    [JsonPropertyName("comment_text")]
    public CommentText CommentText { get; set; }

    [JsonPropertyName("story_title")]
    public StoryTitle StoryTitle { get; set; }

    [JsonPropertyName("story_url")]
    public StoryUrl StoryUrl { get; set; }
}

public class Hit
{
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("title")]
    public object Title { get; set; }

    [JsonPropertyName("url")]
    public object Url { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("points")]
    public int? Points { get; set; }

    [JsonPropertyName("story_text"), JsonConverter(typeof(HtmlDecodeConverter))]
    public string StoryText { get; set; }

    [JsonPropertyName("comment_text"), JsonConverter(typeof(HtmlDecodeConverter))]
    public string CommentText { get; set; }

    [JsonPropertyName("num_comments")]
    public int? NumComments { get; set; }

    [JsonPropertyName("story_id")]
    public int StoryId { get; set; }

    [JsonPropertyName("story_title")]
    public string StoryTitle { get; set; }

    [JsonPropertyName("story_url")]
    public string StoryUrl { get; set; }

    [JsonPropertyName("parent_id")]
    public int ParentId { get; set; }

    [JsonPropertyName("created_at_i")]
    public int CreatedAtI { get; set; }

    [JsonPropertyName("relevancy_score")]
    public int RelevancyScore { get; set; }

    [JsonPropertyName("_tags")]
    public List<string> Tags { get; set; } = new List<string>();

    [JsonPropertyName("objectID")]
    public string ObjectID { get; set; }

    [JsonPropertyName("_highlightResult")]
    public HighlightResult HighlightResult { get; set; }
}

public class ProcessingTimingsMS
{
    [JsonPropertyName("afterFetch")]
    public AfterFetch AfterFetch { get; set; }

    [JsonPropertyName("fetch")]
    public Fetch Fetch { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class HackerNewsResponse
{
    [JsonPropertyName("hits")]
    public IReadOnlyList<Hit> Hits { get; set; } = new List<Hit>();

    [JsonPropertyName("nbHits")]
    public int NbHits { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("nbPages")]
    public int NbPages { get; set; }

    [JsonPropertyName("hitsPerPage")]
    public int HitsPerPage { get; set; }

    [JsonPropertyName("exhaustiveNbHits")]
    public bool ExhaustiveNbHits { get; set; }

    [JsonPropertyName("exhaustiveTypo")]
    public bool ExhaustiveTypo { get; set; }

    [JsonPropertyName("exhaustive")]
    public Exhaustive Exhaustive { get; set; }

    [JsonPropertyName("query")]
    public string Query { get; set; }

    [JsonPropertyName("params")]
    public string Params { get; set; }

    [JsonPropertyName("processingTimeMS")]
    public int ProcessingTimeMS { get; set; }

    [JsonPropertyName("processingTimingsMS")]
    public ProcessingTimingsMS ProcessingTimingsMS { get; set; }
}

public class StoryTitle
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("matchLevel")]
    public string MatchLevel { get; set; }

    [JsonPropertyName("matchedWords")]
    public List<object> MatchedWords { get; set; } = new List<object>();
}

public class StoryUrl
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("matchLevel")]
    public string MatchLevel { get; set; }

    [JsonPropertyName("matchedWords")]
    public List<object> MatchedWords { get; set; } = new List<object>();
}

public class HtmlDecodeConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new Exception("Invalid value type");
        }

        var str = reader.GetString();

        return HttpUtility.HtmlDecode(str);
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}