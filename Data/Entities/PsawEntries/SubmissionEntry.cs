using System;
using Newtonsoft.Json;
using PsawSharp.Converters;

namespace PsawSharp.Entries
{
    public class SubmissionEntry : IEntry
    {

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }

        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; set; }

        [JsonProperty("author_flair_type")]
        public string AuthorFlairType { get; set; }

        [JsonProperty("author_fullname")]
        public string AuthorFullname { get; set; }

        [JsonProperty("author_patreon_flair")]
        public bool AuthorPatreonFlair { get; set; }

        [JsonProperty("can_mod_post")]
        public bool CanModPost { get; set; }

        [JsonProperty("contest_mode")]
        public bool ContestMode { get; set; }

        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime CreatedUtc { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("full_link")]
        public string FullLink { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("is_crosspostable")]
        public bool IsCrosspostable { get; set; }

        [JsonProperty("is_meta")]
        public bool IsMeta { get; set; }

        [JsonProperty("is_original_content")]
        public bool IsOriginalContent { get; set; }

        [JsonProperty("is_reddit_media_domain")]
        public bool IsRedditMediaDomain { get; set; }

        [JsonProperty("is_robot_indexable")]
        public bool IsRobotIndexable { get; set; }

        [JsonProperty("is_self")]
        public bool IsSelf { get; set; }

        [JsonProperty("is_video")]
        public bool IsVideo { get; set; }

        [JsonProperty("link_flair_background_color")]
        public string LinkFlairBackgroundColor { get; set; }

        [JsonProperty("link_flair_css_class")]
        public string LinkFlairCssClass { get; set; }

        [JsonProperty("link_flair_template_id")]
        public string LinkFlairTemplateId { get; set; }

        [JsonProperty("link_flair_text")]
        public string LinkFlairText { get; set; }

        [JsonProperty("link_flair_text_color")]
        public string LinkFlairTextColor { get; set; }

        [JsonProperty("link_flair_type")]
        public string LinkFlairType { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }

        [JsonProperty("media_only")]
        public bool MediaOnly { get; set; }

        [JsonProperty("no_follow")]
        public bool NoFollow { get; set; }

        [JsonProperty("num_comments")]
        public int NumComments { get; set; }

        [JsonProperty("num_crossposts")]
        public int NumCrossposts { get; set; }

        [JsonProperty("over_18")]
        public bool Over18 { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        [JsonProperty("pinned")]
        public bool Pinned { get; set; }

        [JsonProperty("retrieved_on")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime RetrievedOn { get; set; }

        [JsonProperty("post_hint")]
        public string PostHint { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("selftext")]
        public string Selftext { get; set; }

        [JsonProperty("send_replies")]
        public bool SendReplies { get; set; }

        [JsonProperty("spoiler")]
        public bool Spoiler { get; set; }

        [JsonProperty("stickied")]
        public bool Stickied { get; set; }

        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }

        [JsonProperty("subreddit_id")]
        public string SubredditId { get; set; }

        [JsonProperty("subreddit_subscribers")]
        public int SubredditSubscribers { get; set; }

        [JsonProperty("subreddit_type")]
        public string SubredditType { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("preview")]
        public Preview Preview { get; set; }

    }

    public class Preview
    {

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("images")]
        public Image[] Images { get; set; }

    }

    public class Image
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("resolutions")]
        public Resolution[] Resolutions { get; set; }

        [JsonProperty("source")]
        public Source Source { get; set; }

        [JsonProperty("variants")]
        public Variants Variants { get; set; }

    }

    public class Source
    {

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

    }

    public class Variants
    {

        [JsonProperty("obfuscated")]
        public Obfuscated Obfuscated { get; set; }

    }

    public class Obfuscated
    {

        [JsonProperty("resolutions")]
        public Resolution[] Resolutions { get; set; }

    }

    public class Resolution
    {

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

    }
}
