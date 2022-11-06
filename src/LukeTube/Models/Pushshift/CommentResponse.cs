using System.Text.Json;
using System.Text.Json.Serialization;

namespace LukeTube.Models.Pushshift
{
    public class CommentResponse
    {
        [JsonPropertyName("data")]
        public IReadOnlyList<PushshiftCommentResponse> Data { get; set; }
    }

    public class PushshiftCommentResponse
    {
        [JsonPropertyName("approved_at_utc")]
        [JsonConverter(typeof(EpochDateTimeJsonConverter))]
        public DateTime? ApprovedAtUtc { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("author_flair_background_color")]
        public string AuthorFlairBackgroundColor { get; set; }

        [JsonPropertyName("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }

        [JsonPropertyName("author_flair_template_id")]
        public string AuthorFlairTemplateId { get; set; }

        [JsonPropertyName("author_flair_text")]
        public string AuthorFlairText { get; set; }

        [JsonPropertyName("author_flair_text_color")]
        public string AuthorFlairTextColor { get; set; }

        [JsonPropertyName("author_flair_type")]
        public string AuthorFlairType { get; set; }

        [JsonPropertyName("author_fullname")]
        public string AuthorFullname { get; set; }

        [JsonPropertyName("author_patreon_flair")]
        public bool AuthorPatreonFlair { get; set; }

        [JsonPropertyName("banned_at_utc")]
        [JsonConverter(typeof(EpochDateTimeJsonConverter))]
        public DateTime? BannedAtUtc { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("can_mod_post")]
        public bool CanModPost { get; set; }

        [JsonPropertyName("collapsed")]
        public bool Collapsed { get; set; }

        [JsonPropertyName("collapsed_reason")]
        public object CollapsedReason { get; set; }

        [JsonPropertyName("created_utc")]
        [JsonConverter(typeof(EpochDateTimeJsonConverter))]
        public DateTime CreatedUtc { get; set; }

        [JsonPropertyName("distinguished")]
        public string Distinguished { get; set; }

        [JsonPropertyName("edited")]
        public bool Edited { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("is_submitter")]
        public bool IsSubmitter { get; set; }

        [JsonPropertyName("link_id")]
        public string LinkId { get; set; }

        [JsonPropertyName("no_follow")]
        public bool NoFollow { get; set; }

        [JsonPropertyName("parent_id")]
        public string ParentId { get; set; }

        [JsonPropertyName("permalink")]
        public string Permalink { get; set; }

        [JsonPropertyName("retrieved_on")]
        [JsonConverter(typeof(EpochDateTimeJsonConverter))]
        public DateTime RetrievedOn { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("send_replies")]
        public bool SendReplies { get; set; }

        [JsonPropertyName("stickied")]
        public bool Stickied { get; set; }

        [JsonPropertyName("subreddit")]
        public string Subreddit { get; set; }

        [JsonPropertyName("subreddit_id")]
        public string SubredditId { get; set; }
    }

    public class EpochDateTimeJsonConverter : JsonConverter<DateTime>
    {
        private static readonly DateTime _epoc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                var utcValue = reader.GetInt64();
                var convertedDate = _epoc.AddSeconds(utcValue);
                return convertedDate;
            }

            throw new JsonException();
        }

        // TODO: should this be seconds or milliseconds, figure out how to test
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var epocMilliseconds = (long)Math.Round((value - _epoc).TotalSeconds, 0);
            writer.WriteNumberValue(epocMilliseconds);
        }
    }
}
