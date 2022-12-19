using System.Text.Json;
using LukeTubeLib.Models.HackerNews.Entities;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.Models.Pushshift.Entities;
using LukeTubeLib.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Nest;

namespace LukeTube.Services
{
    public interface IPushshiftService
    {
        Task<IReadOnlyList<string>> GetAllSubredditNames();
        Task<IReadOnlyList<RedditComment>> GetAllRedditComments();
        Task<int> GetSubredditLinkCount(string subredditName);
        Task<int> GetTotalRedditComments();
        Task<IReadOnlyList<string>> GetYoutubeIdsBySubreddit(string subredditName);
        Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subreddit);
        Task<IReadOnlyList<CommentViewModel>> GetPagedRedditCommentsBySubreddit(string subredditName, int pageNumber);
        Task<IReadOnlyList<SubredditWithCount>> GetSubredditsWithLinkCounts();
    }

    public sealed class PushshiftService : IPushshiftService
    {
        private readonly IPushshiftRepository _pushshiftRepository;
        private readonly IDistributedCache _distributedCache;

        private static string? s_cachingEnabled => Environment.GetEnvironmentVariable("ENABLE_CACHING");

        public PushshiftService(
            IPushshiftRepository pushshiftRepository,
            IDistributedCache distributedCache)
        {
            _pushshiftRepository = pushshiftRepository;
            _distributedCache = distributedCache;
        }

        public async Task<IReadOnlyList<string>> GetAllSubredditNames()
        {
            var cachedSubredditNames = await _distributedCache.GetAsync($"{nameof(GetAllSubredditNames)}");

            if (cachedSubredditNames is not null && s_cachingEnabled.Equals("true"))
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

            if (allCachedYoutubeIds is not null && s_cachingEnabled.Equals("true"))
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

            var cachedComments = await _distributedCache.GetAsync($"{subreddit}_{nameof(GetYoutubeIdsBySubreddit)}");

            if (cachedComments is not null && s_cachingEnabled.Equals("true"))
            {
                using var stream = new MemoryStream(cachedComments);
                var deserializedCachedComments = await JsonSerializer.DeserializeAsync<IReadOnlyList<RedditComment>>(stream) ?? new List<RedditComment>();

                return deserializedCachedComments;
            }

            var comments = await _pushshiftRepository.GetCommentsBySubreddit(subreddit);
            var json = JsonSerializer.SerializeToUtf8Bytes(comments);
            await _distributedCache.SetAsync($"{subreddit}_{nameof(GetYoutubeIdsBySubreddit)}", json);

            return comments;
        }

        public async Task<IReadOnlyList<CommentViewModel>> GetPagedRedditCommentsBySubreddit(string subredditName, int pageNumber)
        {
            // if (string.IsNullOrEmpty(subredditName)) throw new NullReferenceException(nameof(subredditName));
            //
            // var cachedComments = await _distributedCache.GetAsync($"{subredditName}_{nameof(GetYoutubeIdsBySubreddit)}");
            //
            // if (cachedComments is not null && s_cachingEnabled.Equals("true"))
            // {
            //     using var stream = new MemoryStream(cachedComments);
            //     var deserializedCachedComments =
            //         await JsonSerializer.DeserializeAsync<IReadOnlyList<CommentViewModel>>(stream) ?? new List<CommentViewModel>();
            //
            //     return deserializedCachedComments.Skip(pageNumber * 8).Take(8).ToList();
            // }
            //
            // var comments = (await _pushshiftRepository.GetCommentsBySubreddit(subredditName))
            //     .DistinctBy(x => x.YoutubeId).ToList();
            //
            // var commentViewModels = CommentViewModelHelper.MapEntityToViewModel(comments);
            //
            // var json = JsonSerializer.SerializeToUtf8Bytes(commentViewModels);
            // await _distributedCache.SetAsync($"{subredditName}_{nameof(GetYoutubeIdsBySubreddit)}", json);
            //
            // return commentViewModels
            //     .OrderByDescending(x => x.Score)
            //     .Skip(pageNumber * 8)
            //     .Take(8)
            //     .ToList();
            //

            var elasticSettings = new ConnectionSettings(new Uri("http://localhost:9200"));

            var elasticClient = new ElasticClient(elasticSettings);

            var result = await elasticClient.SearchAsync<CommentViewModel>(s => s
                .Index("pushshift-reddit-comment-index")
                .From(8 * pageNumber)
                .Size(8)
                .Sort(so => so
                    .Descending(p => p.Score))
                .Query(q => q.Term(t => t.Subreddit, subredditName.ToLowerInvariant()))
            );

            return result.Documents.DistinctBy(x => x.YoutubeId).ToList();
        }

        public async Task<IReadOnlyList<string>> GetYoutubeIdsBySubreddit(string subredditName)
        {
            if (string.IsNullOrEmpty(subredditName)) return new List<string>();

            var cachedValue = await _distributedCache.GetAsync($"{subredditName}_{nameof(GetYoutubeIdsBySubreddit)}");

            if (cachedValue is not null && s_cachingEnabled.Equals(true))
            {
                using var stream = new MemoryStream(cachedValue);
                var cachedYoutubeIds = await JsonSerializer.DeserializeAsync<IReadOnlyList<string>>(stream) ?? new List<string>();

                return cachedYoutubeIds;
            }

            var youtubeIds = await _pushshiftRepository.GetYoutubeIdsBySubreddit(subredditName);
            var json = JsonSerializer.SerializeToUtf8Bytes(youtubeIds);
            await _distributedCache.SetAsync($"{subredditName}_{nameof(GetYoutubeIdsBySubreddit)}", json);

            return youtubeIds;
        }

        /// <summary>
        /// Does not return subreddits that do not have any links.
        /// </summary>
        public async Task<IReadOnlyList<SubredditWithCount>> GetSubredditsWithLinkCounts()
        {
            // var cachedSubredditNames = await _distributedCache.GetAsync($"{nameof(GetSubredditsWithLinkCounts)}");
            //
            // if (cachedSubredditNames is not null && s_cachingEnabled.Equals("true"))
            // {
            //     using var stream = new MemoryStream(cachedSubredditNames);
            //     var deserializedSubredditNames =
            //         await JsonSerializer.DeserializeAsync<IReadOnlyList<SubredditWithCount>>(stream)
            //         ?? throw new Exception($"Error deserializing cached value in {nameof(GetSubredditsWithLinkCounts)}.");
            //
            //     return deserializedSubredditNames;
            // }

            // var subredditsWithCount = new List<SubredditWithCount>();
            //
            // // only return subreddits that have any content
            // foreach (var subreddit in AllSubreddits.Subreddits)
            // {
            //     var linkCount = await _pushshiftRepository.GetSubredditLinkCount(subreddit);
            //     if (linkCount > 0) subredditsWithCount.Add(new SubredditWithCount { Subreddit = subreddit, Count = linkCount });
            // }
            //
            // var distinctSubredditsWithCounts = subredditsWithCount.DistinctBy(x => x.Subreddit).ToList();
            var elasticSettings = new ConnectionSettings(new Uri("http://localhost:9200"));

            var elasticClient = new ElasticClient(elasticSettings);

            var result = await elasticClient.SearchAsync<CommentViewModel>(s => s
                .Index("pushshift-reddit-comment-index")
                .Aggregations(a => a
                            .Terms("subreddits", sr => sr
                                .Field("subreddit.keyword")
                                .Size(10000))));
            var items = result.Aggregations.Terms("subreddits").Buckets.Select(x => new SubredditWithCount
            {
                Subreddit = x.Key,
                Count = (int)x.DocCount
            }).ToList();

            var result2 = await elasticClient.SearchAsync<CommentViewModel>(s => s
                .Index("pushshift-reddit-comment-index")
                .Aggregations(a => a
                    .Cardinality("subreddit_count", sr => sr
                        .Field("subreddit.keyword"))));

            return items;
            // var subredditsWithCount = await _pushshiftRepository.GetSubredditsWithCount();
            //
            // var json = JsonSerializer.SerializeToUtf8Bytes(subredditsWithCount);
            // await _distributedCache.SetAsync($"{nameof(GetSubredditsWithLinkCounts)}", json);
            //
            // return subredditsWithCount;
        }


        public Task<int> GetSubredditLinkCount(string subredditName)
        {
            return _pushshiftRepository.GetSubredditLinkCount(subredditName);
        }

        public Task<int> GetTotalRedditComments()
            => _pushshiftRepository.GetTotalRedditComments();
    }
}