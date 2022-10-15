using System;

namespace LukeTube.Data.Entities
{
    public class RedditComment
    {
        public int Id { get; set; }

        public string Subreddit { get; set; }

        public string YoutubeLinkId { get; set; }

        public int Score { get; set; }

        public DateTime CreatedUTC { get; set; }

        public DateTime RetrievedUTC { get; set; }

        public string Permalink { get; set; }
    }
}
