using System.Text.Json.Serialization;

namespace LukeTubeLib.Models.HackerNews;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public record HackerNewsAuthor(
    [property: JsonPropertyName("value")] string value,
    [property: JsonPropertyName("matchLevel")] string matchLevel,
    [property: JsonPropertyName("matchedWords")] IReadOnlyList<object> matchedWords
);

public record HighlightResult(
    [property: JsonPropertyName("title")] Title title,
    [property: JsonPropertyName("author")] Author author,
    [property: JsonPropertyName("story_text")] StoryText story_text
);

public record Hit(
    [property: JsonPropertyName("created_at")] DateTime created_at,
    [property: JsonPropertyName("title")] string title,
    [property: JsonPropertyName("url")] object url,
    [property: JsonPropertyName("author")] string author,
    [property: JsonPropertyName("points")] int points,
    [property: JsonPropertyName("story_text")] string story_text,
    [property: JsonPropertyName("comment_text")] object comment_text,
    [property: JsonPropertyName("num_comments")] int num_comments,
    [property: JsonPropertyName("story_id")] object story_id,
    [property: JsonPropertyName("story_title")] object story_title,
    [property: JsonPropertyName("story_url")] object story_url,
    [property: JsonPropertyName("parent_id")] object parent_id,
    [property: JsonPropertyName("created_at_i")] int created_at_i,
    [property: JsonPropertyName("_tags")] IReadOnlyList<string> _tags,
    [property: JsonPropertyName("objectID")] string objectID,
    [property: JsonPropertyName("_highlightResult")] HighlightResult _highlightResult
);

public record Root(
    [property: JsonPropertyName("hits")] IReadOnlyList<Hit> hits
);

public record StoryText(
    [property: JsonPropertyName("value")] string value,
    [property: JsonPropertyName("matchLevel")] string matchLevel,
    [property: JsonPropertyName("fullyHighlighted")] bool fullyHighlighted,
    [property: JsonPropertyName("matchedWords")] IReadOnlyList<string> matchedWords
);

public record Title(
    [property: JsonPropertyName("value")] string value,
    [property: JsonPropertyName("matchLevel")] string matchLevel,
    [property: JsonPropertyName("matchedWords")] IReadOnlyList<object> matchedWords
);

public class HackerNewsResponse
{
    [JsonPropertyName("hits")]
    public IReadOnlyList<Hit>? Hits { get; set; }
}

// public class Hit
// {
//     [JsonPropertyName("created_at")]
//     // public DateTime CreatedAt { get; set; }
//     public string CreatedAt { get; set; }
//     [JsonPropertyName("title")]
//     public string Title { get; set; }
//     [JsonPropertyName("url")]
//     public string Url { get; set; }
//     [JsonPropertyName("author")]
//     public string Author { get; set; }
//     [JsonPropertyName("points")]
//     public int Points { get; set; }
//     [JsonPropertyName("story_text")]
//     public string StoryText { get; set; }
//     [JsonPropertyName("comment_text")]
//     public string CommentText { get; set; }
//     [JsonPropertyName("num_comments")]
//     public int NumComments { get; set; }
//     [JsonPropertyName("story_id")]
//     // public DateTime StoryId { get; set; }
//     public int? StoryId { get; set; }
//     [JsonPropertyName("story_title")]
//     public string StoryTitle { get; set; }
//     [JsonPropertyName("story_url")]
//     public string StoryUrl { get; set; }
//     [JsonPropertyName("parent_id")]
//     // public DateTime ParentId { get; set; }
//     public string ParentId { get; set; }
//     [JsonPropertyName("created_at_i")]
//     // public DateTime CreatedAtI { get; set; }
//     public string CreatedAtI { get; set; }
//     [JsonPropertyName("objectID")]
//     public string ObjectId { get; set; }
// }