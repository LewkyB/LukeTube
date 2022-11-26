using LukeTubeLib.Models.Pushshift;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YoutubeExplode.Videos;

namespace LukeTubeLib.Repositories
{
    public interface IPushshiftRepository
    {
        Task<IReadOnlyList<string>> GetAllSubredditNames();
        Task<IReadOnlyList<RedditComment>> GetAllRedditComments();
        Task<int> GetSubredditLinkCount(string subredditName);
        Task<int> GetTotalRedditComments();
        Task<IReadOnlyList<string>> GetYoutubeIdsBySubreddit(string subredditName);
        Task SaveRedditComments(IReadOnlyList<RedditComment> redditComments);
        Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subredditName);
        Task<IReadOnlyList<SubredditWithCount>> GetSubredditsWithCount();
    }

    public sealed class PushshiftRepository : IPushshiftRepository
    {
        private readonly PushshiftContext _pushshiftContext;
        private readonly ILogger<PushshiftRepository> _logger;

        public PushshiftRepository(
            PushshiftContext pushshiftContext,
            ILogger<PushshiftRepository> logger)
        {
            _pushshiftContext = pushshiftContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<RedditComment>> GetAllRedditComments()
        {
            return await _pushshiftContext.RedditComments
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetAllSubredditNames()
        {
            return await _pushshiftContext.RedditComments
                .Select(comment => comment.Subreddit)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetYoutubeIdsBySubreddit(string subredditName)
        {
            return await _pushshiftContext.RedditComments
                .Where(comment => comment.Subreddit == subredditName)
                .Select(comment => comment.YoutubeLinkId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subredditName)
        {
            // TODO: better way than a lot of includes?
            return await _pushshiftContext.RedditComments
                .AsNoTracking()
                .Where(comment => comment.Subreddit == subredditName)
                .Include(x => x.VideoModel)
                .Include(x => x.VideoModel.Author)
                .Include(x => x.VideoModel.EngagementModel)
                .Include(x => x.VideoModel.Thumbnails)
                .ToListAsync();
        }

        public async Task<int> GetSubredditLinkCount(string subredditName)
        {
            return await _pushshiftContext.RedditComments
                .Where(comment => comment.Subreddit == subredditName)
                .AsNoTracking()
                .CountAsync();
        }

        public async Task SaveRedditComments(IReadOnlyList<RedditComment> redditComments)
        {
            foreach (var comment in redditComments)
            {
                // TODO: this is synchronous, probably causing some blocking behavior since everything else is async
                // var exists = _pushshiftContext.RedditComments.Local.FirstOrDefault(x => x.YoutubeLinkId == comment.YoutubeLinkId);
                //
                // // perform identity resolution to prevent tracking duplicate key exception
                // if (exists is not null)
                // {
                //     exists.YoutubeLinkId = comment.YoutubeLinkId;
                // }
                // else
                // {
                    var isInDatabase = await _pushshiftContext.RedditComments
                        .AnyAsync(x => x.YoutubeLinkId == comment.YoutubeLinkId && x.Subreddit == comment.Subreddit);

                    // TODO: ew nested if
                    if (!isInDatabase) await _pushshiftContext.RedditComments.AddAsync(comment);
                // }
            }

            await _pushshiftContext.SaveChangesAsync();
        }

        public async Task<int> GetTotalRedditComments()
        {
            return await _pushshiftContext.RedditComments
                .AsNoTracking()
                .CountAsync();
        }

        public async Task<IReadOnlyList<SubredditWithCount>> GetSubredditsWithCount()
        {
            return await _pushshiftContext.RedditComments
                .GroupBy(x => x.Subreddit)
                .Select(x => new SubredditWithCount
                {
                    Subreddit = x.Key,
                    Count = x.Count(),
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }
}