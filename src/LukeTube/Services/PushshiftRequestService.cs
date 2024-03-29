﻿using LukeTube.Data;
using System.Text.RegularExpressions;
using LukeTube.Models.Pushshift;

namespace LukeTube.Services
{
    public interface IPushshiftRequestService
    {
        Task<IReadOnlyList<string>> GetSubreddits();
        Task GetUniqueRedditComments(IReadOnlyList<string> subreddit, int daysToGet, int numEntriesPerDay);
    }
    public sealed class PushshiftRequestService : IPushshiftRequestService
    {
        private readonly ILogger<PushshiftRequestService> _logger;
        private readonly IPushshiftRepository _pushshiftRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string YoutubeLinkIdRegexPattern = @"http(?:s?):\/\/(?:www\.)?youtu(?:be\.com\/watch\?v=|\.be\/)([\w\-\]*)(&(amp;)?[\w\?‌​=]*)?";
        private static readonly Regex YoutubeLinkIdRegex = new(
            YoutubeLinkIdRegexPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(30));

        public PushshiftRequestService(
            ILogger<PushshiftRequestService> logger,
            IPushshiftRepository pushshiftRepository,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _pushshiftRepository = pushshiftRepository;
            _httpClientFactory = httpClientFactory;
        }

        // TODO: provide a better list of subreddits
        // avoid magic strings
        private static readonly List<string> Subreddits = new()
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
            foreach (var subreddit in Subreddits)
            {
                var linkCount = await _pushshiftRepository.GetSubredditLinkCount(subreddit);
                if (linkCount > 0) validSubreddits.Add(subreddit);
            }

            // return list in alphabetical order
            return validSubreddits.OrderBy(x => x).ToList();
        }

        public async Task GetUniqueRedditComments(IReadOnlyList<string> subreddits, int daysToGet, int numEntriesPerDay)
        {
            if (subreddits is null) throw new NullReferenceException(nameof(subreddits));

            var redditComments = new List<RedditComment>();

            // going by hour gets more detailed results
            var daysToGetInHours = daysToGet * 24;
            for (var i = 0; i < daysToGetInHours; i++)
            {
                var before = daysToGetInHours - i + "h";
                var after = daysToGetInHours + 1 - i + "h";

                var rawComments = await GetPushshiftQueryResults<CommentResponse>("comment", new SearchOptions
                {
                    Subreddit = string.Join(",", subreddits),
                    Query = "www.youtube.com/watch", // TODO: separate out the query for the other link and score
                    Before = before,
                    After = after,
                    Size = numEntriesPerDay
                });

                _logger.LogTrace($"{i} out of {daysToGetInHours}\tFetched {rawComments.Data.Count}\tBefore:{daysToGetInHours - i} After:{(daysToGetInHours + 1) - i}");

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

                await _pushshiftRepository.SaveUniqueComments(redditComments);
            }
        }

        internal async Task<T> GetPushshiftQueryResults<T>(string requestType, SearchOptions? searchOptions = null) where T : new()
        {
            var pushshiftUrl = requestType switch
            {
                "comment" => $"reddit/comment/search?{ArgsToString(searchOptions.ToArgs())}",
                "submission" => $"reddit/submission/search?{ArgsToString(searchOptions.ToArgs())}",
                "meta" => "meta",
                _ => ""
            };

            var httpClient = _httpClientFactory.CreateClient("PushshiftServiceClient");
            var results = new T();

            try
            {
                // TODO: what is the better option to returning empty object?
                results = await httpClient.GetFromJsonAsync<T>(pushshiftUrl) ?? new T();
            }
            // TODO: best way to handle failure?
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure to retrieve data from {httpClient.BaseAddress + pushshiftUrl}");
            }

            return results;
        }

        // should this return multiples? do i need to change youtube linkid to be an array
        internal string FindYoutubeId(string commentBody)
        {
            if (string.IsNullOrEmpty(commentBody)) return string.Empty;

            var linkMatches = YoutubeLinkIdRegex.Matches(commentBody);

            // TODO: what happens when there is multiple youtube links in a body? is there something to do with that
            foreach (Match match in linkMatches)
            {
                if (match is null || match.Value.Equals(""))
                {
                    _logger.LogInformation($"Match is null or empty: {match} {match?.Value}");
                    return string.Empty;
                }

                var groups = match.Groups;

                if (groups.Count > 2)
                {
                    _logger.LogInformation($"Caught more than one youtube id {string.Join(",", groups.Values)}");
                }

                if (match.Groups[1].Length < 11) return string.Empty;

                // trim down id, it should be a maximum of 11 characters
                return groups[1].Length > 11 ? groups[1].Value.Remove(11) : groups[1].Value;
            }

            return string.Empty;
        }

        private static string ArgsToString(IEnumerable<string> args)
            => args.Aggregate((x, y) => $"{x}&{y}");
    }
}