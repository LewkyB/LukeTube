using LukeTubeLib.Models.HackerNews.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LukeTubeLib.Repositories
{
    public interface IHackerNewsRepository
    {
        // Task<IReadOnlyList<string>> GetAllSubredditNames();
        // Task<IReadOnlyList<RedditComment>> GetAllRedditComments();
        // Task<int> GetSubredditLinkCount(string subredditName);
        // Task<int> GetTotalRedditComments();
        // Task<IReadOnlyList<string>> GetYoutubeIdsBySubreddit(string subredditName);
        Task SaveHackerNewsHits(IReadOnlyList<HackerNewsHit> hackerNewsHits);
        // Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subredditName);
        // Task<IReadOnlyList<SubredditWithCount>> GetSubredditsWithCount();
    }

    public sealed class HackerNewsRepository : IHackerNewsRepository
    {
        private readonly HackerNewsContext _hackerNewsContext;
        private readonly ILogger<HackerNewsRepository> _logger;

        public HackerNewsRepository(HackerNewsContext hackerNewsContext, ILogger<HackerNewsRepository> logger)
        {
            _hackerNewsContext = hackerNewsContext;
            _logger = logger;
        }

        // public async Task<IReadOnlyList<RedditComment>> GetAllRedditComments()
        // {
        //     return await _hackerNewsContext.RedditComments
        //         .AsNoTracking()
        //         .ToListAsync();
        // }
        //
        // public async Task<IReadOnlyList<string>> GetAllSubredditNames()
        // {
        //     return await _pushshiftContext.RedditComments
        //         .Select(comment => comment.Subreddit)
        //         .Distinct()
        //         .AsNoTracking()
        //         .ToListAsync();
        // }
        //
        // public async Task<IReadOnlyList<string>> GetYoutubeIdsBySubreddit(string subredditName)
        // {
        //     return await _pushshiftContext.RedditComments
        //         .Where(comment => comment.Subreddit == subredditName)
        //         .Select(comment => comment.YoutubeId)
        //         .AsNoTracking()
        //         .ToListAsync();
        // }
        //
        // public async Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subredditName)
        // {
        //     // TODO: better way than a lot of includes?
        //     return await _pushshiftContext.RedditComments
        //         .AsNoTracking()
        //         .Where(comment => comment.Subreddit == subredditName)
        //         .Include(x => x.VideoModel)
        //         .Include(x => x.VideoModel.Author)
        //         .Include(x => x.VideoModel.EngagementModel)
        //         .Include(x => x.VideoModel.Thumbnails)
        //         .ToListAsync();
        // }
        //
        // public async Task<int> GetSubredditLinkCount(string subredditName)
        // {
        //     return await _pushshiftContext.RedditComments
        //         .Where(comment => comment.Subreddit == subredditName)
        //         .AsNoTracking()
        //         .CountAsync();
        // }

        public async Task SaveHackerNewsHits(IReadOnlyList<HackerNewsHit> hackerNewsHits)
        {
            foreach (var hackerNewsHit in hackerNewsHits)
            {
                var isInDatabase = await _hackerNewsContext.HackerNewsHits
                    .AnyAsync(x => x.YoutubeId == hackerNewsHit.YoutubeId).ConfigureAwait(false);

                // TODO: ew nested if
                if (!isInDatabase) await _hackerNewsContext.HackerNewsHits.AddAsync(hackerNewsHit).ConfigureAwait(false);
            }

            var totalWrites = await _hackerNewsContext.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation($"Total number of writes: {totalWrites}");
        }

        // public async Task<int> GetTotalRedditComments()
        // {
        //     return await _pushshiftContext.RedditComments
        //         .AsNoTracking()
        //         .CountAsync();
        // }
        //
        // public async Task<IReadOnlyList<SubredditWithCount>> GetSubredditsWithCount()
        // {
        //     return await _pushshiftContext.RedditComments
        //         .GroupBy(x => x.Subreddit)
        //         .Select(x => new SubredditWithCount
        //         {
        //             Subreddit = x.Key,
        //             Count = x.Count(),
        //         })
        //         .AsNoTracking()
        //         .ToListAsync();
        // }
    }
}