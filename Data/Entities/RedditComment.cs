// Example of comment structure from pushshift can be found here:
// https://api.pushshift.io/reddit/comment/search/?q=favorite+great+courses&after=2015-01-01

using System;
using Microsoft.EntityFrameworkCore;

namespace luke_site_mvc.Data.Entities
{
    //[Index(nameof(Subreddit), nameof(YoutubeLinkId), IsUnique = true)]
    public class RedditComment
    {
        //public int Id { get; set; } // is there a point to having an id?

        // 'subreddit'
        public string Subreddit { get; set; }

        // regex from 'body'
        // careful for surprise EF behavior on something with Id at the end?
        public string YoutubeLinkId { get; set; }

        // 'score'
        public int Score { get; set; }

        // 'link_id' how to turn this into a reddit link
        public string CommentLink { get; set; }

        // 'created_utc'
        public DateTime CreatedUTC { get; set; }

        // 'retrieved_on'
        public DateTime RetrievedUTC { get; set; }
    }
}
