using luke_site_mvc.Data;
using luke_site_mvc.Data.Entities;
using luke_site_mvc.Models.PsawSearchOptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PsawSharp.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace luke_site_mvc.Services
{
    public interface IPushshiftService
    {
        string FindYoutubeId(string commentBody);
        Task GetLinksFromCommentsAsync();
        List<string> GetSubreddits();
        Task<List<RedditComment>> GetUniqueRedditComments(string subreddit, int daysToGet, int numEntriesPerDay);
    }
    public class PushshiftService : IPushshiftService
    {
        private readonly ILogger<PushshiftService> _logger;
        private readonly IDistributedCache _cache;
        private readonly IPsawService _psawService;
        private readonly ISubredditRepository _subredditRepository;

        public PushshiftService(ILogger<PushshiftService> logger, IDistributedCache distributedCache, IPsawService psawService, ISubredditRepository subredditRepository)
        {
            _logger = logger;
            _cache = distributedCache;
            _psawService = psawService;
            _subredditRepository = subredditRepository;
        }

        // TODO: provide a better list of subreddits
        // avoid magic strings
        private readonly List<string> subreddits = new List<string>()
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
                "askculinary"
        };

        //private readonly List<string> subreddits = new List<string>()
        //{
        //        "homeimprovement",
        //        "woodworking",
        //        "medicine",
        //        "ultralight",
        //        "travel",
        //        "askHistorians",
        //        "askculinary"
        //};

        public List<string> GetSubreddits()
        {
            return subreddits.OrderBy(x => x).ToList(); // sort the list alphabetically
        }

        public async Task GetLinksFromCommentsAsync()
        {
            foreach(var subreddit in subreddits)
            {
                var redditComments = await GetUniqueRedditComments(subreddit, daysToGet: 1, numEntriesPerDay: 10);

                _subredditRepository.SaveUniqueComments(redditComments);
            }
        }

        public async Task<List<RedditComment>> GetUniqueRedditComments(string subreddit, int daysToGet, int numEntriesPerDay)
        {
            if (String.IsNullOrEmpty(subreddit)) throw new NullReferenceException(nameof(subreddit));

            var redditComments = new List<RedditComment>();

            // to get a specific day, like the 25th
            var beforeBoundary = DateTime.Now.AddDays(1); // before the 26th
            var afterBoundary = DateTime.Now.AddDays(-1); // after the 24th

            for (int i = 0; i < daysToGet; i++)
            {
                var rawComments = await _psawService.Search<CommentEntry>(new SearchOptions
                {
                    Subreddit = subreddit,
                    Query = "www.youtube.com/watch", // TODO: seperate out the query for the other link and score
                    Before = beforeBoundary.AddDays( -i ).ToString("yyyy-MM-dd"),
                    After = afterBoundary.AddDays( -i ).ToString("yyyy-MM-dd"),
                    Size = numEntriesPerDay
                });

                foreach (var comment in rawComments)
                {
                    // check to make sure comment body has a valid YoutubeLinkId in it
                    var validated_link = FindYoutubeId(comment.Body);

                    // if not valid YoutubeLinkId then do not continue
                    if (String.IsNullOrEmpty(validated_link)) break;

                    // load up RedditComment with data from the API response
                    RedditComment redditComment = new RedditComment
                    {
                        Subreddit = comment.Subreddit,
                        YoutubeLinkId = FindYoutubeId(comment.Body), // use regex to pull youtubeId from comment body
                        CreatedUTC = comment.CreatedUtc,
                        Score = comment.Score,
                        RetrievedUTC = comment.RetrievedOn
                    };

                    redditComments.Add(redditComment);
                }
            }

            // remove any duplicate comments
            return redditComments.Distinct().ToList();
        }

        static readonly string youtubeLinkIdRegexPattern = @"http(?:s?):\/\/(?:www\.)?youtu(?:be\.com\/watch\?v=|\.be\/)([\w\-\]*)(&(amp;)?[\w\?‌​=]*)?";

        // worth compiling because this regex is used so heavily
        Regex youtubeLinkIdRegex = new Regex(youtubeLinkIdRegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // should this return multiples? do i need to change youtube linkid to be an array
        // TODO: worried about performance on the regex here?
        public string FindYoutubeId(string commentBody)
        {
            if (String.IsNullOrEmpty(commentBody)) return String.Empty;

            MatchCollection link_matches;
            link_matches = youtubeLinkIdRegex.Matches(commentBody);

            // TODO: what happens when there is multiple youtube links in a body? is there something to do with that
            foreach (Match match in link_matches)
            {
                if (match is null || match.Equals(""))
                {
                    // TODO: how to get method name in log message?
                    _logger.LogTrace("FindYoutubeId(string commentBody) | invalid link, breaking loop");
                    return "";
                }
                GroupCollection groups = match.Groups;

                if (match.Groups[1].Length < 11) return String.Empty;

                // trim down id, it should be a maximum of 11 characters
                return (groups[1].Length > 11) ? groups[1].Value.Remove(11) : groups[1].Value;
            }

            return String.Empty;
        }
    }
}
