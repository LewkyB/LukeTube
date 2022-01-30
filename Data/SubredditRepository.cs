using luke_site_mvc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace luke_site_mvc.Data
{
    public interface ISubredditRepository
    {
        Task<IReadOnlyList<string>> GetAllSubredditNames();
        Task<IReadOnlyList<RedditComment>> GetAllYoutubeIDs();
        int GetSubredditLinkCount(string subredditName);
        int GetTotalRedditComments();
        IQueryable<RedditComment> GetYoutubeIDsBySubreddit(string subredditName);
        void SaveUniqueComments(List<RedditComment> redditComments);
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
        public IQueryable<RedditComment> GetYoutubeIDsBySubreddit(string subredditName)
        {
            return _subredditContext.RedditComments
                .Where(comment => comment.Subreddit == subredditName)
                .Select(comment => comment);
        }

        public int GetSubredditLinkCount(string subredditName)
        {
            return _subredditContext.RedditComments
                .Where(comment => comment.Subreddit == subredditName)
                .Select(comment => comment)
                .Count();
        }

        // TODO: get async database calls to work w/o concurrency issues
        // makes sure that there isn't a duplicate entry that has the same
        // Subreddit and YoutubeLinkId combination in the database before
        // inserting a new record
        public void SaveUniqueComments(List<RedditComment> redditComments)
        {
            foreach (var comment in redditComments)
            {
                if (!_subredditContext.RedditComments.Any(c => c.Subreddit == comment.Subreddit && c.YoutubeLinkId == comment.YoutubeLinkId))
                {
                    _subredditContext.RedditComments.Add(comment);
                    _subredditContext.SaveChanges();
                }
            }
        }

        public int GetTotalRedditComments()
        {
            return _subredditContext.RedditComments.Count();
        }
    }
}
