using LukeTube.Data;
using LukeTube.Data.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LukeTube.Data.Entities.PsawEntries.PsawSearchOptions;
using StackExchange.Profiling.Internal;

namespace LukeTube.Services
{
    public interface IPushshiftService
    {
        string FindYoutubeId(string commentBody);
        Task GetLinksFromCommentsAsync();
        Task<IReadOnlyList<string>> GetSubreddits();
        Task GetUniqueRedditComments(List<string> subreddit, int daysToGet, int numEntriesPerDay);
    }
    public class PushshiftService : IPushshiftService
    {
        private readonly ILogger<PushshiftService> _logger;
        private readonly IDistributedCache _cache;
        private readonly ISubredditRepository _subredditRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string _youtubeLinkIdRegexPattern = @"http(?:s?):\/\/(?:www\.)?youtu(?:be\.com\/watch\?v=|\.be\/)([\w\-\]*)(&(amp;)?[\w\?‌​=]*)?";
        private static readonly Regex _youtubeLinkIdRegex = new(
            _youtubeLinkIdRegexPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(30));

        public PushshiftService(
            ILogger<PushshiftService> logger,
            IDistributedCache distributedCache,
            ISubredditRepository subredditRepository,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _cache = distributedCache;
            _subredditRepository = subredditRepository;
            _httpClientFactory = httpClientFactory;
        }

        // TODO: provide a better list of subreddits
        // avoid magic strings
        private readonly List<string> _subreddits = new()
        {
            "space",
            "science",
            "mealtimevideos",
            "skookum",
            "artisanvideos",
            "AIDKE",
            "linux",
            "movies",
            "dotnet",
            "csharp",
            "biology",
            "astronomy",
            "photography",
            "aviation",
            "lectures",
            "homebrewing",
            "fantasy",
            "homeimprovement",
            "woodworking",
            "medicine",
            "ultralight",
            "travel",
            "askHistorians",
            "camping",
            "cats",
            "cpp",
            "chemistry",
            "beer",
            "whisky",
            "games",
            "moviesuggestions",
            "utarlington",
            "docker",
            "dotnetcore",
            "math",
            "askculinary",
            "tesla",
            "nintendoswitch",
            "diy",
            "aww",
            "history",
            "youtube",
            "askscience",
            "dallas",
            "galveston",
            "arlington",
            "programming",
            "arch",
            "buildapcsales",
            "cooking",
            "hunting",
            "askculinary",
            "blender",
            "CC0Textures",
            "DigitalArt",
            "blenderTutorials",
            "computergraphics",
            "3Dmodeling",
            "blenderhelp",
            "cgiMemes",
            "learnblender",
            "blenderpython",
            "low_poly",
            "ancienthistory",
            "AncientWorld",
            "Anthropology",
            "ArtefactPorn",
            "aviationmaintenance",
            "badhistory",
            "commandline",
            "contalks",
            "dailyprogrammer",
            "finallydeclassified",
            "fsharp",
            "indoorgarden",
            "vegetablegardening",
            "submarines",
            "systemd",
            "smoking",
            "grilling",
            "salsasnobs",
            "python",
            "powershell",
            "osdev",
            "homelab",
            "experienceddevs",
        };

            // shorter list while dealing with rate limit problems
            //private readonly List<string> _subreddits = new()
            //{
            //    "homeimprovement",
            //    "woodworking",
            //    "medicine",
            //    "ultralight",
            //    "travel",
            //    "askculinary",
            //    "space",
            //    "skookum",
            //    "AIDKE",
            //    "linux",
            //    "dotnet",
            //    "csharp",
            //    "biology",
            //    "aviation",
            //    "homebrewing",
            //    "fantasy",
            //    "whisky",
            //    "docker",
            //    "math",
            //    "history",
            //    "mealtimevideos",
            //    "movies",
            //    "gaming",
            //    "youtube",
            //};

        public async Task<IReadOnlyList<string>> GetSubreddits()
        {
            var validSubreddits = new List<string>();

            // only return subreddits that have any content
            foreach (var subreddit in _subreddits)
            {
                var linkCount = await _subredditRepository.GetSubredditLinkCount(subreddit);
                if (linkCount > 0) validSubreddits.Add(subreddit);
            }

            // return list in alphabetical order
            return validSubreddits.OrderBy(x => x).ToList();
        }

        public async Task GetLinksFromCommentsAsync()
            => await GetUniqueRedditComments(_subreddits, daysToGet: 365, numEntriesPerDay: 100);

        public async Task GetUniqueRedditComments(List<string> subreddits, int daysToGet, int numEntriesPerDay)
        {
            if (subreddits is null) throw new NullReferenceException(nameof(subreddits));

            var redditComments = new List<RedditComment>();

            var subredditCsv = string.Join(",", subreddits);

            // going by hour gets more detailed results
            var daysToGetInHours = daysToGet * 24;
            for (var i = 0; i < daysToGetInHours; i++)
            {
                string before = daysToGetInHours - i + "h";
                string after = (daysToGetInHours + 1 - i) + "h";

                // var rawComments = await _psawService.Search<CommentEntry>(new SearchOptions
                var rawComments = await GetData(new() {
                    Subreddit = subredditCsv,
                    Query = "www.youtube.com/watch", // TODO: seperate out the query for the other link and score
                    Before = before,
                    After = after,
                    Size = numEntriesPerDay
                });

                _logger.LogInformation($"{i} out of {daysToGetInHours}\tFetched {rawComments.Data.Count()}\tBefore:{daysToGetInHours - i} After:{(daysToGetInHours + 1) - i}");

                foreach (var comment in rawComments.Data.Distinct())
                {
                    // check to make sure comment body has a valid YoutubeLinkId
                    var youtubeId = FindYoutubeId(comment.Body);

                    // if not valid YoutubeLinkId then do not continue
                    if (string.IsNullOrEmpty(youtubeId)) break;

                    var redditComment = new RedditComment {
                        Subreddit = comment.Subreddit,
                        YoutubeLinkId = youtubeId,
                        CreatedUTC = comment.CreatedUtc,
                        Score = comment.Score,
                        RetrievedUTC = comment.RetrievedOn,
                        Permalink = comment.Permalink
                    };

                    redditComments.Add(redditComment);
                }

                await _subredditRepository.SaveUniqueComments(redditComments);
            }
        }

        public async Task<PushshiftCommentResponseModel> GetData(SearchOptions searchOptions)
        {
            var httpClient = _httpClientFactory.CreateClient("PushshiftServiceCommentClient");
            return await httpClient .GetFromJsonAsync<PushshiftCommentResponseModel>($"?{ArgsToString(searchOptions.ToArgs())}");
        }

        // should this return multiples? do i need to change youtube linkid to be an array
        public string FindYoutubeId(string commentBody)
        {
            if (string.IsNullOrEmpty(commentBody)) return string.Empty;

            var linkMatches = _youtubeLinkIdRegex.Matches(commentBody);

            // TODO: what happens when there is multiple youtube links in a body? is there something to do with that
            foreach (Match match in linkMatches)
            {
                if (match is null || match.Value.Equals(""))
                {
                    _logger.LogInformation($"Match is null or empty: {match} {match?.Value}");
                    return string.Empty;
                }

                GroupCollection groups = match.Groups;

                if (groups.Count > 2)
                {
                    _logger.LogInformation("Caught more than one youtube id {}", groups.ToJson());
                }

                if (match.Groups[1].Length < 11) return string.Empty;

                // trim down id, it should be a maximum of 11 characters
                return (groups[1].Length > 11) ? groups[1].Value.Remove(11) : groups[1].Value;
            }

            return string.Empty;
        }

        private static string ConstructUrl(string route, IReadOnlyCollection<string> args)
            => args == null ? route : $"{route}?{ArgsToString(args)}";

        private static string ArgsToString(IEnumerable<string> args)
            => args.Aggregate((x, y) => $"{x}&{y}");
    }

    public static class RequestsConstants
    {
        public const string BaseAddress = "https://api.pushshift.io/";
        public const string SearchRoute = "reddit/{0}/search";
        public const string CommentIdsRoute = "reddit/submission/comment_ids/{0}";
    }
}
