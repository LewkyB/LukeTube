namespace LukeTube.Models.Pushshift
{
    public class RedditComment
    {
        public int Id { get; set; }

        public string Subreddit { get; init; }

        public string YoutubeLinkId { get; init; }

        public int Score { get; set; }

        public DateTime CreatedUTC { get; set; }

        public DateTime RetrievedUTC { get; set; }

        public string Permalink { get; set; }
    }
}
