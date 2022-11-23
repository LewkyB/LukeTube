using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YoutubeExplode.Videos;

namespace LukeTubeLib.Models.Pushshift;

[PrimaryKey(nameof(VideoId))]
public class VideoModel
{
    public int VideoId { get; set; }
    public string YoutubeId { get; set; }

    public string Url => $"https://www.youtube.com/watch?v={YoutubeId}";

    public string Title { get; set; }

    public Author Author { get; set; }

    /// <summary>
    /// Video upload date.
    /// </summary>
    public DateTimeOffset UploadDate { get; set; }

    /// <summary>
    /// Video description.
    /// </summary>
    public string Description { get; set; }

    public TimeSpan? Duration { get; set; }

    public List<Thumbnail> Thumbnails { get; set; }

    /// <summary>
    /// Available search keywords for the video.
    /// </summary>
    public List<string> Keywords { get; set; }

    /// <summary>
    /// Engagement statistics for the video.
    /// </summary>
    public EngagementModel EngagementModel { get; set; }
    // [ForeignKey(nameof(RedditCommentId))]
    // public RedditComment RedditComment { get; set; }
    // public int RedditCommentId { get; set; }


}

public class Author
{
    [Key]
    public int AuthorId { get; set; }

    /// <summary>
    /// Channel ID.
    /// </summary>
    public string ChannelId { get; set; }

    /// <summary>
    /// Channel URL.
    /// </summary>
    public string ChannelUrl => $"https://www.youtube.com/channel/{ChannelId}";

    /// <summary>
    /// Channel title.
    /// </summary>
    public string ChannelTitle { get; set; }

    // public VideoModel VideoModel { get; set; }
    // [ForeignKey(nameof(VideoId))]
    // public int VideoId { get; set; }
}

[PrimaryKey(nameof(ThumbnailId))]
public class Thumbnail
{
    public int ThumbnailId;
    public string Url { get; set; }

    /// <summary>
    /// Thumbnail resolution.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Canvas height (in pixels).
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Canvas area (width multiplied by height).
    /// </summary>
    public int Area => Width * Height;

    // [ForeignKey(nameof(VideoId))]
    // public VideoModel VideoModel { get; set; }
    // public int VideoId { get; set; }
}

public class EngagementModel
{
    [Key]
    public int EngagementId { get; set; }
    /// <summary>
    /// View count.
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// Like count.
    /// </summary>
    public long LikeCount { get; set; }

    /// <summary>
    /// Dislike count.
    /// </summary>
    /// <remarks>
    /// YouTube no longer supports dislikes, so this value is always 0.
    /// </remarks>
    public long DislikeCount { get; set; }

    /// <summary>
    /// Average rating.
    /// </summary>
    /// <remarks>
    /// YouTube no longer supports dislikes, so this value is always 5.
    /// </remarks>
    public double AverageRating => LikeCount + DislikeCount != 0
        ? 1 + 4.0 * LikeCount / (LikeCount + DislikeCount)
        : 0; // avoid division by 0

    // [ForeignKey(nameof(VideoId))]
    // public VideoModel VideoModel { get; set; }
    // public int VideoId { get; set; }
}

public static class VideoModelHelper
{
    public static VideoModel MapVideoEntity(Video video)
    {
        var thumbnailModels = new List<Thumbnail>();
        foreach (var thumbnail in video.Thumbnails)
        {
            var newThumbnail = new Thumbnail
            {
                Height = thumbnail.Resolution.Height,
                Width = thumbnail.Resolution.Width,
                Url = thumbnail.Url,
            };
            thumbnailModels.Add(newThumbnail);
        }

        return new VideoModel
        {
            YoutubeId = video.Id.Value,
            Duration = video.Duration,
            EngagementModel = new EngagementModel
            {
                ViewCount = video.Engagement.ViewCount,
                DislikeCount = video.Engagement.DislikeCount,
                LikeCount = video.Engagement.LikeCount,
            },
            Author = new Author
            {
                ChannelId = video.Author.ChannelId.Value,
                ChannelTitle = video.Author.ChannelTitle,
            },
            Description = video.Description,
            Keywords = video.Keywords.ToList(),
            Title = video.Title,
            Thumbnails = thumbnailModels,
            UploadDate = video.UploadDate.ToUniversalTime()
        };
    }
}