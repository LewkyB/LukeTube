using LukeTubeLib.Models.Pushshift.Entities;

namespace LukeTubeLib.Models.Pushshift;

public class CommentViewModel
{
    public string Subreddit { get; init; }

    public string YoutubeId { get; set; }

    public int Score { get; set; }

    public DateTime CreatedUTC { get; set; }

    public DateTime RetrievedUTC { get; set; }

    public string Permalink { get; set; }

    public VideoViewModel VideoModel { get; set; }
}

// public class VideoViewModel
// {
//     public string YoutubeId { get; set; }
//
//     public string Url => $"https://www.youtube.com/watch?v={YoutubeId}";
//
//     public string Title { get; set; }
//
//     public AuthorViewModel Author { get; set; }
//
//     /// <summary>
//     /// Video upload date.
//     /// </summary>
//     public DateTimeOffset UploadDate { get; set; }
//
//     /// <summary>
//     /// Video description.
//     /// </summary>
//     public string Description { get; set; }
//
//     public TimeSpan? Duration { get; set; }
//
//     public List<ThumbnailViewModel> Thumbnails { get; set; }
//
//     /// <summary>
//     /// Available search keywords for the video.
//     /// </summary>
//     public List<string> Keywords { get; set; }
//
//     /// <summary>
//     /// Engagement statistics for the video.
//     /// </summary>
//     public EngagementViewModel Engagement { get; set; }
// }
//
// public class AuthorViewModel
// {
//     public string ChannelId { get; set; }
//
//     public string ChannelUrl => $"https://www.youtube.com/channel/{ChannelId}";
//
//     /// <summary>
//     /// Channel title.
//     /// </summary>
//     public string ChannelTitle { get; set; }
// }
//
// public class ThumbnailViewModel
// {
//     public string Url { get; set; }
//
//     public int Width { get; set; }
//
//     public int Height { get; set; }
//
//     public int Area => Width * Height;
// }
//
// public class EngagementViewModel
// {
//     public long ViewCount { get; set; }
//
//     public long LikeCount { get; set; }
//
//     public long DislikeCount { get; set; }
//
//     public double AverageRating => LikeCount + DislikeCount != 0
//         ? 1 + 4.0 * LikeCount / (LikeCount + DislikeCount)
//         : 0; // avoid division by 0
// }

public static class CommentViewModelHelper
{
    public static IReadOnlyList<CommentViewModel> MapEntityToViewModel(IReadOnlyList<RedditComment> redditComments)
    {
        var commentViewModels = new List<CommentViewModel>();

        foreach (var redditComment in redditComments)
        {
            commentViewModels.Add(new CommentViewModel
            {
                YoutubeId = redditComment.YoutubeId,
                Subreddit = redditComment.Subreddit,
                Permalink = redditComment.Permalink,
                Score = redditComment.Score,
                CreatedUTC = redditComment.CreatedUTC,
                RetrievedUTC = redditComment.RetrievedUTC,
                VideoModel = new VideoViewModel
                {
                    YoutubeId = redditComment.VideoModel.YoutubeId,
                    Title = redditComment.VideoModel.Title,
                    Description = redditComment.VideoModel.Description,
                    Keywords = redditComment.VideoModel.Keywords,
                    Duration = redditComment.VideoModel.Duration,
                    UploadDate = redditComment.VideoModel.UploadDate,
                    Author = new AuthorViewModel
                    {
                        ChannelId = redditComment.VideoModel.Author.ChannelId,
                        ChannelTitle = redditComment.VideoModel.Author.ChannelTitle
                    },
                    Thumbnails = redditComment.VideoModel.Thumbnails.Select(x => new ThumbnailViewModel
                    {
                        Height = x.Height,
                        Width = x.Width,
                        Url = x.Url
                    }).ToList(),
                    Engagement = new EngagementViewModel
                    {
                        DislikeCount = redditComment.VideoModel.EngagementModel.DislikeCount,
                        LikeCount = redditComment.VideoModel.EngagementModel.LikeCount,
                        ViewCount = redditComment.VideoModel.EngagementModel.ViewCount
                    }
                }
            });
        }

        return commentViewModels;
    }
}