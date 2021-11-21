using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using luke_site_mvc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Data
{
    public interface ISubredditRepository
    {
        Task<IReadOnlyList<string>> GetAllSubredditNames();
        Task<IReadOnlyList<RedditComment>> GetAllYoutubeIDs();
        Task<IReadOnlyList<string>> GetYoutubeIDsBySubreddit(string subredditName);
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

        public async Task<IReadOnlyList<string>> GetYoutubeIDsBySubreddit(string subredditName)
        {
            return await _subredditContext.RedditComments
                //.OrderByDescending(comment => comment.Id)
                .Where(comment => comment.Subreddit == subredditName)
                .Select(comment => comment.YoutubeLinkId)
                .ToListAsync();
        }

    }
}
