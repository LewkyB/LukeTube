using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LukeTubeLib.Models.Pushshift.Entities
{
    [PrimaryKey(nameof(RedditCommentId))]
    public class RedditComment
    {
        [Key]
        public int RedditCommentId { get; set; }

        [Required]
        public string Subreddit { get; init; }

        [Required]
        public string YoutubeId { get; set; }

        public int Score { get; set; }

        public DateTime CreatedUTC { get; set; }

        public DateTime RetrievedUTC { get; set; }

        public string Permalink { get; set; }
        public VideoModel VideoModel { get; set; }
    }
}