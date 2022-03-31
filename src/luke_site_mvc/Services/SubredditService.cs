using luke_site_mvc.Data;
using luke_site_mvc.Data.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace luke_site_mvc.Services
{
    public interface ISubredditService
    {
        Task<IReadOnlyList<string>> GetAllSubredditNames();
        Task<IReadOnlyList<RedditComment>> GetAllYoutubeIDs();
        int GetSubredditLinkCount(string subredditName);
        int GetTotalRedditComments();
        IQueryable<RedditComment> GetYouLinkIDsBySubreddit(string subredditName);
    }

    // TODO: make this have more of a point than just returning readonlylists
    public class SubredditService : ISubredditService
    {
        private readonly ISubredditRepository _subredditRepository;
        private readonly ILogger<SubredditService> _logger;

        public SubredditService(ILogger<SubredditService> logger, ISubredditRepository dataRepository)
        {
            _subredditRepository = dataRepository;

            _logger = logger;
            _logger.LogDebug(1, "NLog injected into ChatroomService");
        }

        public async Task<IReadOnlyList<string>> GetAllSubredditNames()
        {
            return await _subredditRepository.GetAllSubredditNames();
        }

        public async Task<IReadOnlyList<RedditComment>> GetAllYoutubeIDs()
        {
            return await _subredditRepository.GetAllYoutubeIDs();
        }

        public IQueryable<RedditComment> GetYouLinkIDsBySubreddit(string subredditName)
        {
            return _subredditRepository.GetYoutubeIDsBySubreddit(subredditName);
        }

        public int GetSubredditLinkCount(string subredditName)
        {
            return _subredditRepository.GetSubredditLinkCount(subredditName);
        }

        public int GetTotalRedditComments()
            => _subredditRepository.GetTotalRedditComments();
    }
}
