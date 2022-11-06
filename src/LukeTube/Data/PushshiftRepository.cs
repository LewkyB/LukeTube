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
        private readonly PushshiftContext _pushshiftContext;

        public PushshiftRepository(PushshiftContext pushshiftContext)
        {
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

        public async Task<int> GetSubredditLinkCount(string subredditName)
        {
            return await _pushshiftContext.RedditComments
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

        public async Task<int> GetTotalRedditComments()
        {
            return await _pushshiftContext.RedditComments.CountAsync();
        }
    }
}