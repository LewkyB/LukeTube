using System.Net;
using System.Net.Http.Json;
using System.Text;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.Repositories;
using Microsoft.Extensions.Logging;

namespace LukeTubeLib.Services
{
    public interface IPushshiftRequestService
    {
        IReadOnlyList<SearchOptions> GetSearchOptions(string query, int daysToGet, int maxNumComments);
        Task<IReadOnlyList<RedditComment>> GetUniqueRedditComments(SearchOptions searchOption, HttpClient? rateLimitedClient);
        // Task<IReadOnlyList<RedditComment>> GetRedditComments(IReadOnlyList<SearchOptions> searchOptions);
        List<SearchOptions> BuildSearchOptionsNoDates(string query, int maxNumComments);
        IList<SearchOptions> AddBeforeAndAfter(IList<SearchOptions> searchOptions, int dayToGet);
    }
    public sealed class PushshiftRequestService : IPushshiftRequestService
    {
        private readonly ILogger<PushshiftRequestService> _logger;
        private readonly IPushshiftRepository _pushshiftRepository;
        private readonly IHttpClientFactory _httpClientFactory;


        public PushshiftRequestService(
            ILogger<PushshiftRequestService> logger,
            IPushshiftRepository pushshiftRepository,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _pushshiftRepository = pushshiftRepository;
            _httpClientFactory = httpClientFactory;
        }

        // TODO: when this runs with 365 days to get and 100 max comments, it allocates about 280mb, how to prevent an allocation that big but still get the results
        public IReadOnlyList<SearchOptions> GetSearchOptions(string query, int daysToGet, int maxNumComments)
        {
            if (string.IsNullOrEmpty(query)) throw new NullReferenceException(nameof(query));

            var searchOptions = new List<SearchOptions>();
            // going by hour gets more detailed results
            var daysToGetInHours = daysToGet * 24;
            for (var i = 0; i < daysToGetInHours; i++)
            {
                var before = daysToGetInHours - i + "h";
                var after = daysToGetInHours + 1 - i + "h";

                searchOptions.AddRange(BuildSearchOptions(query, maxNumComments, before, after));
            }

            return searchOptions;
        }

        public async Task<IReadOnlyList<RedditComment>> GetUniqueRedditComments(SearchOptions searchOption, HttpClient? rateLimitedClient)
        {
            var redditComments = new List<RedditComment>();

            // List<HttpClient> clients = new List<HttpClient>();
            // for (int i = 0; i < 10; i++)
            // {
            //     clients.Add(_httpClientFactory.CreateClient("PushshiftRequestServiceClient"));
            // }
            //
            // var tasks = new List<Task>();
            // var semaphore = new SemaphoreSlim(10);
            // foreach (HttpClient client in clients)
            // {
            //     await semaphore.WaitAsync();
            //     try
            //     {
            //         tasks.Add(GetPushshiftQueryResults<CommentResponse>("comment", searchOption));
            //     }
            //     finally
            //     {
            //         semaphore.Release();
            //     }
            // }
            // await Task.WhenAll(tasks);

            var rawComments =
                (await GetPushshiftQueryResults<CommentResponse>("comment", searchOption, rateLimitedClient)).Data.Distinct();

            // if (rawComments is null) return redditComments;

            foreach (var comment in rawComments)
            {
                var youtubeIds = FindYoutubeId(comment.Body);
                redditComments.AddRange( CreateRedditComments(youtubeIds, comment));
            }

            return redditComments.DistinctBy(x => x.YoutubeLinkId).ToList();
        }

        public List<SearchOptions> BuildSearchOptionsNoDates(string query, int maxNumComments)
        {
            var searchOptions = new List<SearchOptions>();
            var stringBuilder = new StringBuilder();
            var counter = 0;

            // < or <=, am I cutting off the last subreddit?
            while (counter < AllSubreddits.Subreddits.Count)
            {
                var searchOption = new SearchOptions
                {
                    Subreddit = string.Empty,
                    Query = query,
                    Before = null,
                    After = null,
                    Size = maxNumComments
                };

                stringBuilder.Append(AllSubreddits.Subreddits[counter++]);

                // get the total length of the search options that will get appended to the URI
                var searchOptionLength = searchOption.ToString().Length;

                // 1950 because we're aiming for a max length of 2000, we want to leave plenty of wiggle room
                // because there is still the URI to consider
                while (stringBuilder.Length < 1950 - searchOptionLength && counter < AllSubreddits.Subreddits.Count)
                {
                    // TODO: better way that interpolation?
                    stringBuilder.Append($",{AllSubreddits.Subreddits[counter++]}");
                }

                searchOption.Subreddit = stringBuilder.ToString();
                stringBuilder.Clear();
                searchOptions.Add(searchOption);
            }

            return searchOptions;
        }

        public IList<SearchOptions> AddBeforeAndAfter(IList<SearchOptions> searchOptions, int dayToGet)
        {
            // 0 days ago
            //initial before
            // 24 * 0(day to get) = 0
            // initial after
            // (24 * 0(day to get)) + 2 = 2
            // add 2 to both 12 times
            var newSearchOptions = new List<SearchOptions>();
            int initialBefore = 24 * dayToGet;
            int initialAfter = 24 * dayToGet + 2;
            for (int i = 0; i < 24; i += 2)
            {
                initialBefore += i;
                initialAfter += i;
                string before = initialBefore + "h";
                string after = initialAfter + "h";


                for (int j = 0; j < searchOptions.Count; j++)
                {
                    var newSearchOption = new SearchOptions
                    {
                        Subreddit = searchOptions[j].Subreddit,
                        After = after,
                        Size = searchOptions[j].Size,
                        Before = before,
                        Query = searchOptions[j].Query,
                    };

                    newSearchOptions.Add(newSearchOption);
                }
            }

            return newSearchOptions;
        }

        internal List<SearchOptions> BuildSearchOptions(string query, int maxNumComments, string before, string after)
        {
            var searchOptions = new List<SearchOptions>();
            var stringBuilder = new StringBuilder();
            var counter = 0;

            // < or <=, am I cutting off the last subreddit?
            while (counter < AllSubreddits.Subreddits.Count)
            {
                var searchOption = new SearchOptions
                {
                    Subreddit = string.Empty,
                    Query = query,
                    Before = before,
                    After = after,
                    Size = maxNumComments
                };

                stringBuilder.Append(AllSubreddits.Subreddits[counter++]);

                // get the total length of the search options that will get appended to the URI
                var searchOptionLength = searchOption.ToString().Length;

                // 1950 because we're aiming for a max length of 2000, we want to leave plenty of wiggle room
                // because there is still the URI to consider
                while (stringBuilder.Length < 1950 - searchOptionLength && counter < AllSubreddits.Subreddits.Count)
                {
                    // TODO: better way that interpolation?
                    stringBuilder.Append($",{AllSubreddits.Subreddits[counter++]}");
                }

                searchOption.Subreddit = stringBuilder.ToString();
                stringBuilder.Clear();
                searchOptions.Add(searchOption);
            }

            return searchOptions;
        }

        internal static IReadOnlyList<RedditComment> CreateRedditComments(IReadOnlyList<string> youtubeIds, PushshiftCommentResponse comment)
        {
            if (youtubeIds.Count <= 0) return new List<RedditComment>();

            var redditComments = new List<RedditComment>();

            foreach (var youtubeId in youtubeIds)
            {
                var redditComment = new RedditComment
                {
                    Subreddit = comment.Subreddit,
                    YoutubeLinkId = youtubeId,
                    CreatedUTC = comment.CreatedUtc,
                    Score = comment.Score,
                    RetrievedUTC = comment.RetrievedOn,
                    Permalink = comment.Permalink
                };

                redditComments.Add(redditComment);
            }

            return redditComments;
        }

        internal async Task<T> GetPushshiftQueryResults<T>(string requestType, SearchOptions? searchOptions, HttpClient? rateLimtedClient) where T : new()
        {
            var pushshiftUrl = requestType switch
            {
                "comment" => $"reddit/comment/search?{ArgsToString(searchOptions.ToArgs())}",
                "submission" => $"reddit/submission/search?{ArgsToString(searchOptions.ToArgs())}",
                "meta" => "meta",
                _ => ""
            };

            // var httpClient = _httpClientFactory.CreateClient("PushshiftRequestServiceClient");
            var httpClient = rateLimtedClient ?? _httpClientFactory.CreateClient("PushshiftRequestServiceClient");

            var result = new T();

            try
            {
                result = await httpClient.GetFromJsonAsync<T>(pushshiftUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return result;
        }

        // should this return multiples? do i need to change youtube linkid to be an array
        internal IReadOnlyList<string> FindYoutubeId(string commentBody)
        {
            if (string.IsNullOrEmpty(commentBody)) return new List<string>();

            if (IsValidYoutubeId(commentBody)) return new []{ commentBody };

            // TODO: this is definitely in the hot path, is LINQ the best choice?
            // TODO: wow there has got to be another way to combine all this regex?
            // TODO: is it overkill to combine all of these or should I add some conditional logic?
            var shortsMatches = ShortsYoutubeRegex.FindYoutubeIdMatches(commentBody);
            var fullMatches = FullYoutubeRegex.FindYoutubeIdMatches(commentBody);
            var normalMatches = NormalYoutubeRegex.FindYoutubeIdMatches(commentBody);
            var minimalMatches = MinimalYoutubeRegex.FindYoutubeIdMatches(commentBody);
            var embeddedMatches = EmbeddedYoutubeRegex.FindYoutubeIdMatches(commentBody);
            var linkMatches = shortsMatches
                .Union(fullMatches)
                .Union(normalMatches)
                .Union(minimalMatches)
                .Union(minimalMatches)
                .Union(embeddedMatches);

            var youtubeIds = new List<string>();
            foreach (var match in linkMatches.DistinctBy(x => x.Groups[1].Value))
            {
                if (IsValidYoutubeId(match.Groups[1].Value)) youtubeIds.Add(match.Groups[1].Value);
            }

            return youtubeIds;
        }

        private static bool IsValidYoutubeId(string videoId) =>
            videoId.Length == 11 &&
            videoId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

        private static string ArgsToString(IEnumerable<string> args)
            => args.Aggregate((x, y) => $"{x}&{y}");
    }
}