using LukeTube.Data;
using System.Text.Json;
using LukeTube.Models.Pushshift;
using Microsoft.Extensions.Caching.Distributed;

namespace LukeTube.Services
{
    public interface IPushshiftService
    {
        Task<IReadOnlyList<string>> GetAllSubredditNames();
        Task<IReadOnlyList<RedditComment>> GetAllRedditComments();
        Task<int> GetSubredditLinkCount(string subredditName);
        Task<int> GetTotalRedditComments();
        Task<IReadOnlyList<string>> GetYoutubeLinkIdsBySubreddit(string subredditName);
        Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subreddit);
    }

    public sealed class PushshiftService : IPushshiftService
    {
        private readonly IPushshiftRepository _pushshiftRepository;
        private readonly IDistributedCache _distributedCache;
        private readonly IConfiguration _configuration;

        private static string? CachingEnabled => Environment.GetEnvironmentVariable("ENABLE_CACHING");
        
        public PushshiftService(
            IPushshiftRepository pushshiftRepository, 
            IDistributedCache distributedCache,
            IConfiguration configuration)
        {
            _pushshiftRepository = pushshiftRepository;
            _distributedCache = distributedCache;
            _configuration = configuration;
        }

        public async Task<IReadOnlyList<string>> GetAllSubredditNames()
        {
            var cachedSubredditNames = await _distributedCache.GetAsync($"{nameof(GetAllSubredditNames)}");

            if (cachedSubredditNames is not null && CachingEnabled.Equals("true"))
            {
                using var stream = new MemoryStream(cachedSubredditNames);
                var deserializedSubredditNames = await JsonSerializer.DeserializeAsync<IReadOnlyList<string>>(stream) ?? new List<string>();

                return deserializedSubredditNames;
            }

            var subredditNames = await _pushshiftRepository.GetAllSubredditNames();
            var json = JsonSerializer.SerializeToUtf8Bytes(subredditNames);
            await _distributedCache.SetAsync($"{nameof(GetAllSubredditNames)}", json);

            return subredditNames;
        }

        public async Task<IReadOnlyList<RedditComment>> GetAllRedditComments()
        {
            var allCachedYoutubeIds = await _distributedCache.GetAsync($"{nameof(GetAllRedditComments)}");

            if (allCachedYoutubeIds is not null && CachingEnabled.Equals("true"))
            {
                using var stream = new MemoryStream(allCachedYoutubeIds);
                var deserializedRedditComments = await JsonSerializer.DeserializeAsync<IReadOnlyList<RedditComment>>(stream) ?? new List<RedditComment>();

                return deserializedRedditComments;
            }

            var allYoutubeIDs = await _pushshiftRepository.GetAllRedditComments();
            var json = JsonSerializer.SerializeToUtf8Bytes(allYoutubeIDs);
            await _distributedCache.SetAsync($"{nameof(GetAllRedditComments)}", json);
            return allYoutubeIDs;
        }

        public async Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subreddit)
        {
            if (string.IsNullOrEmpty(subreddit)) return new List<RedditComment>();

            var cachedComments = await _distributedCache.GetAsync($"{subreddit}_{nameof(GetYoutubeLinkIdsBySubreddit)}");

            if (cachedComments is not null && CachingEnabled.Equals("true"))
            {
                using var stream = new MemoryStream(cachedComments);
                var deserializedCachedComments = await JsonSerializer.DeserializeAsync<IReadOnlyList<RedditComment>>(stream) ?? new List<RedditComment>();

                return deserializedCachedComments;
            }
            
            var comments = await _pushshiftRepository.GetCommentsBySubreddit(subreddit);
            var json = JsonSerializer.SerializeToUtf8Bytes(comments);
            await _distributedCache.SetAsync($"{subreddit}_{nameof(GetYoutubeLinkIdsBySubreddit)}", json);

            return comments;
        }

        public async Task<IReadOnlyList<string>> GetYoutubeLinkIdsBySubreddit(string subredditName)
        {
            if (string.IsNullOrEmpty(subredditName)) return new List<string>();
            
            var cachedValue = await _distributedCache.GetAsync($"{subredditName}_{nameof(GetYoutubeLinkIdsBySubreddit)}");

            if (cachedValue is not null && CachingEnabled.Equals(true))
            {
                using var stream = new MemoryStream(cachedValue);
                var cachedYoutubeIds = await JsonSerializer.DeserializeAsync<IReadOnlyList<string>>(stream) ?? new List<string>();

                return cachedYoutubeIds;
            }

            var youtubeIds = await _pushshiftRepository.GetYoutubeIdsBySubreddit(subredditName);
            var json = JsonSerializer.SerializeToUtf8Bytes(youtubeIds);
            await _distributedCache.SetAsync($"{subredditName}_{nameof(GetYoutubeLinkIdsBySubreddit)}", json);

            return youtubeIds;
        }

        public Task<int> GetSubredditLinkCount(string subredditName)
        {
            return _pushshiftRepository.GetSubredditLinkCount(subredditName);
        }

        public Task<int> GetTotalRedditComments()
            => _pushshiftRepository.GetTotalRedditComments();
    }
}