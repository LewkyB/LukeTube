using LukeTube.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LukeTube.Data
{
    public interface ISubredditRepository
    {
        Task<IReadOnlyList<string>> GetAllSubredditNames();
        Task<IReadOnlyList<RedditComment>> GetAllYoutubeIDs();
        Task<int> GetSubredditLinkCount(string subredditName);
        Task<int> GetTotalRedditComments();
        Task<IReadOnlyList<string>> GetYoutubeIdsBySubreddit(string subredditName);
        Task SaveUniqueComments(IReadOnlyList<RedditComment> redditComments);
        Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subredditName);
    }

    public class SubredditRepository : ISubredditRepository
    {
        private readonly ILogger<SubredditRepository> _logger;
        private readonly IConfiguration _config;
        private readonly SubredditContext _subredditContext;

        public SubredditRepository(IConfiguration config, ILogger<SubredditRepository> logger, SubredditContext subredditContext)
        {
            _config = config;
            _logger = logger;
            _subredditContext = subredditContext;
        }

        public async Task<IReadOnlyList<RedditComment>> GetAllYoutubeIDs()
        {
            return await _subredditContext.RedditComments
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetAllSubredditNames()
        {
            return await _subredditContext.RedditComments
                .Where(comment => comment.Subreddit != " ")
                .Select(comment => comment.Subreddit).Distinct()
                .ToListAsync();
        }

        // TODO: adding sorting logic here or in service layer
        public async Task<IReadOnlyList<string>> GetYoutubeIdsBySubreddit(string subredditName)
        {
            return await _subredditContext.RedditComments
                .Where(comment => comment.Subreddit == subredditName)
                .Select(comment => comment.YoutubeLinkId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subredditName)
        {
            return await _subredditContext.RedditComments
                .Where(comment => comment.Subreddit == subredditName)
                .ToListAsync();
        }

        public Task<int> GetSubredditLinkCount(string subredditName)
        {
            return _subredditContext.RedditComments
                .Where(comment => comment.Subreddit == subredditName)
                .Select(comment => comment)
                .CountAsync();
        }

        // TODO: get async database calls to work w/o concurrency issues
        // makes sure that there isn't a duplicate entry that has the same
        // Subreddit and YoutubeLinkId combination in the database before
        // inserting a new record
        public async Task SaveUniqueComments(IReadOnlyList<RedditComment> redditComments)
        {
            foreach (var comment in redditComments)
            {
                var exists = await _subredditContext.RedditComments
                    .AnyAsync(c => c.Subreddit == comment.Subreddit && c.YoutubeLinkId == comment.YoutubeLinkId);

                if (!exists)
                {
                    await _subredditContext.RedditComments.AddAsync(comment);
                    await _subredditContext.SaveChangesAsync();
                }
            }
        }

        public Task<int> GetTotalRedditComments()
        {
            return _subredditContext.RedditComments.CountAsync();
        }
    }
}
