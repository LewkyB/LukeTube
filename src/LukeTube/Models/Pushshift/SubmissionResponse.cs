using System.Text.Json.Serialization;

namespace LukeTube.Models.Pushshift
{
    public class SubmissionResponse
    {
        [JsonPropertyName("data")]
        public IReadOnlyList<PushshiftSubmissions> Data { get; set; }
    }

    public class PushshiftSubmissions
    {
        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }

        [JsonPropertyName("author_flair_text")]
        public string AuthorFlairText { get; set; }

        [JsonPropertyName("author_flair_type")]
        public string AuthorFlairType { get; set; }

        [JsonPropertyName("author_fullname")]
        public string AuthorFullname { get; set; }

        [JsonPropertyName("author_patreon_flair")]
        public bool AuthorPatreonFlair { get; set; }

        [JsonPropertyName("can_mod_post")]
        public bool CanModPost { get; set; }

        [JsonPropertyName("contest_mode")]
        public bool ContestMode { get; set; }

        [JsonPropertyName("created_utc")]
        [JsonConverter(typeof(EpochDateTimeJsonConverter))]
        public DateTime CreatedUtc { get; set; }

        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        [JsonPropertyName("full_link")]
        public string FullLink { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("is_crosspostable")]
        public bool IsCrosspostable { get; set; }

        [JsonPropertyName("is_meta")]
        public bool IsMeta { get; set; }

        [JsonPropertyName("is_original_content")]
        public bool IsOriginalContent { get; set; }

        [JsonPropertyName("is_reddit_media_domain")]
        public bool IsRedditMediaDomain { get; set; }

        [JsonPropertyName("is_robot_indexable")]
        public bool IsRobotIndexable { get; set; }

        [JsonPropertyName("is_self")]
        public bool IsSelf { get; set; }

        [JsonPropertyName("is_video")]
        public bool IsVideo { get; set; }

        [JsonPropertyName("link_flair_background_color")]
        public string LinkFlairBackgroundColor { get; set; }

        [JsonPropertyName("link_flair_css_class")]
        public string LinkFlairCssClass { get; set; }

        [JsonPropertyName("link_flair_template_id")]
        public string LinkFlairTemplateId { get; set; }

        [JsonPropertyName("link_flair_text")]
        public string LinkFlairText { get; set; }

        [JsonPropertyName("link_flair_text_color")]
        public string LinkFlairTextColor { get; set; }

        [JsonPropertyName("link_flair_type")]
        public string LinkFlairType { get; set; }

        [JsonPropertyName("locked")]
        public bool Locked { get; set; }

        [JsonPropertyName("media_only")]
        public bool MediaOnly { get; set; }

        [JsonPropertyName("no_follow")]
        public bool NoFollow { get; set; }

        [JsonPropertyName("num_comments")]
        public int NumComments { get; set; }

        [JsonPropertyName("num_crossposts")]
        public int NumCrossposts { get; set; }

        [JsonPropertyName("over_18")]
        public bool Over18 { get; set; }

        [JsonPropertyName("permalink")]
        public string Permalink { get; set; }

        [JsonPropertyName("pinned")]
        public bool Pinned { get; set; }

        [JsonPropertyName("retrieved_on")]
        [JsonConverter(typeof(EpochDateTimeJsonConverter))]
        public DateTime RetrievedOn { get; set; }

        [JsonPropertyName("post_hint")]
        public string PostHint { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("selftext")]
        public string Selftext { get; set; }

        [JsonPropertyName("send_replies")]
        public bool SendReplies { get; set; }

        [JsonPropertyName("spoiler")]
        public bool Spoiler { get; set; }

        [JsonPropertyName("stickied")]
        public bool Stickied { get; set; }

        [JsonPropertyName("subreddit")]
        public string Subreddit { get; set; }

        [JsonPropertyName("subreddit_id")]
        public string SubredditId { get; set; }

        [JsonPropertyName("subreddit_subscribers")]
        public int SubredditSubscribers { get; set; }

        [JsonPropertyName("subreddit_type")]
        public string SubredditType { get; set; }

        [JsonPropertyName("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("preview")]
        public Preview Preview { get; set; }
    }

    public class Preview
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("images")]
        public Image[] Images { get; set; }
    }

    public class Image
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("resolutions")]
        public Resolution[] Resolutions { get; set; }

        [JsonPropertyName("source")]
        public Source Source { get; set; }

        [JsonPropertyName("variants")]
        public Variants Variants { get; set; }
    }

    public class Source
    {
        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }
    }

    public class Variants
    {
        [JsonPropertyName("obfuscated")]
        public Obfuscated Obfuscated { get; set; }
    }

    public class Obfuscated
    {
        [JsonPropertyName("resolutions")]
        public Resolution[] Resolutions { get; set; }
    }

    public class Resolution
    {
        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }
    }
}
