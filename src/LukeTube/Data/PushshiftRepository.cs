using Microsoft.EntityFrameworkCore;
using LukeTube.Models.Pushshift;

namespace LukeTube.Data
{
    public interface IPushshiftRepository
    {
        Task<IReadOnlyList<string>> GetAllSubredditNames();
        Task<IReadOnlyList<RedditComment>> GetAllRedditComments();
        Task<int> GetSubredditLinkCount(string subredditName);
        Task<int> GetTotalRedditComments();
        Task<IReadOnlyList<string>> GetYoutubeIdsBySubreddit(string subredditName);
        Task SaveUniqueComments(IReadOnlyList<RedditComment> redditComments);
        Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subredditName);
    }

    public sealed class PushshiftRepository : IPushshiftRepository
    {
        private readonly ILogger<PushshiftRepository> _logger;
        private readonly IConfiguration _config;
        private readonly PushshiftContext _pushshiftContext;

        public PushshiftRepository(IConfiguration config, ILogger<PushshiftRepository> logger, PushshiftContext pushshiftContext)
        {
            _config = config;
            _logger = logger;
            _pushshiftContext = pushshiftContext;
        }

        public async Task<IReadOnlyList<RedditComment>> GetAllRedditComments()
        {
            return await _pushshiftContext.RedditComments
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetAllSubredditNames()
        {
            return await _pushshiftContext.RedditComments
                .Where(comment => comment.Subreddit != " ")
                .Select(comment => comment.Subreddit).Distinct()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetYoutubeIdsBySubreddit(string subredditName)
        {
            return await _pushshiftContext.RedditComments
                .Where(comment => comment.Subreddit == subredditName)
                .Select(comment => comment.YoutubeLinkId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subredditName)
        {
            return await _pushshiftContext.RedditComments
                .Where(comment => comment.Subreddit == subredditName)
                .ToListAsync();
        }

        public Task<int> GetSubredditLinkCount(string subredditName)
        {
            return _pushshiftContext.RedditComments
                .Where(comment => comment.Subreddit == subredditName)
                .Select(comment => comment)
                .CountAsync();
        }

        public async Task SaveUniqueComments(IReadOnlyList<RedditComment> redditComments)
        {
            foreach (var comment in redditComments)
            {
                var exists = await _pushshiftContext.RedditComments
                    .AnyAsync(c => c.Subreddit == comment.Subreddit && c.YoutubeLinkId == comment.YoutubeLinkId);

                if (!exists)
                {
                    await _pushshiftContext.RedditComments.AddAsync(comment);
                    await _pushshiftContext.SaveChangesAsync();
                }
            }
        }

        public Task<int> GetTotalRedditComments()
        {
            return _pushshiftContext.RedditComments.CountAsync();
        }
    }
}
