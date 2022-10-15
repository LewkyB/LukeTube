using LukeTube.Data;
using LukeTube.Data.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LukeTube.Services
{
    public interface ISubredditService
    {
        Task<IReadOnlyList<string>> GetAllSubredditNames();
        Task<IReadOnlyList<RedditComment>> GetAllYoutubeIDs();
        int GetSubredditLinkCount(string subredditName);
        int GetTotalRedditComments();
        IReadOnlyList<string> GetYoutubeLinkIdsBySubreddit(string subredditName);
        Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subreddit);
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

        public async Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subreddit)
        {
            return await _subredditRepository.GetCommentsBySubreddit(subreddit);
        }

        public IReadOnlyList<string> GetYoutubeLinkIdsBySubreddit(string subredditName)
        {
            return _subredditRepository.GetYoutubeIdsBySubreddit(subredditName);
        }

        public int GetSubredditLinkCount(string subredditName)
        {
            return _subredditRepository.GetSubredditLinkCount(subredditName);
        }

        public int GetTotalRedditComments()
            => _subredditRepository.GetTotalRedditComments();
    }
}
