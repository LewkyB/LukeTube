using luke_site_mvc.Data;
using luke_site_mvc.Data.Entities;
using luke_site_mvc.Models.PsawSearchOptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PsawSharp.Entries;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace luke_site_mvc.Services
{
    public interface IPushshiftService
    {
        string FindYoutubeId(string commentBody);
        Task<List<RedditComment>> GetLinksFromCommentsAsync(string selected_subreddit, string order = "desc");
        Task<List<RedditComment>> GetRedditComments(List<RedditComment> redditComments, List<string> subreddits, int numDays);
        Task<List<string>> GetSubreddits();
    }
    public class PushshiftService : IPushshiftService
    {
        private readonly ILogger<PushshiftService> _logger;
        private readonly IDistributedCache _cache;
        private readonly SubredditContext _subredditContext;
        private readonly IPsawService _psawService;

        public PushshiftService(ILogger<PushshiftService> logger, IDistributedCache distributedCache, SubredditContext subredditContext, IPsawService psawService)
        {
            _logger = logger;
            _cache = distributedCache;
            _subredditContext = subredditContext;
            _psawService = psawService;
        }
        public async Task<List<string>> GetSubreddits()
        {
            // TODO: provide a better list of subreddits

            // the shortest of lists
            //List<string> subreddits = new List<string>() { "mealtimevideos" };

            // long list
            List<string> subreddits = new List<string>()
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
                "woodworking"
            };

            // shorter list
            //List<string> subreddits = new List<string>()
            //{
            //    "space",
            //    "science",
            //    "mealtimevideos",
            //    "skookum",
            //    "artisanvideos",
            //    "AIDKE",
            //    "linux",
            //    "homeimprovement",
            //    "woodworking"
            //};

            // sort the list alphabetically
            return subreddits.OrderBy(x => x).ToList();
        }

        // TODO: try to move all of the database calls into a repository
        public async Task<List<RedditComment>> GetLinksFromCommentsAsync(string selected_subreddit, string order = "desc")
        {
            List<RedditComment> redditComments = new List<RedditComment>();

            List<string> subreddits = await GetSubreddits();

            await GetRedditComments(redditComments, subreddits, 2);

            // remove any duplicants from the list of RedditComments
            redditComments = redditComments.Distinct().ToList();

            // used to hold remaining RedditComments after duplicate entries are filtered out
            List<RedditComment> redditCommentsNoDuplicateEntries = new List<RedditComment>();

            // TODO: can I make this more efficient? 
            // filter out any entries that are already in the database 
            // that have the same Subreddit and YoutubeLinkId combination
            foreach (var comment in redditComments)
            {
                if (!_subredditContext.RedditComments.Any(
                    c => c.Subreddit == comment.Subreddit && c.YoutubeLinkId == comment.YoutubeLinkId))
                {
                    redditCommentsNoDuplicateEntries.Add(comment);
                }
            }

            // load up the database with the new data and save
            await _subredditContext.AddRangeAsync(redditCommentsNoDuplicateEntries);
            await _subredditContext.SaveChangesAsync();

            // TODO: this won't be necessary once the pages get data from the database instead of directly from the API
            // sort comments so that the highest scored video shows at the top
            List<RedditComment> commentsSorted = new List<RedditComment>();

            // default sort order is descending
            if (order.Equals("desc"))
            {
                commentsSorted = redditComments.OrderByDescending(m => m.Score).ToList();
            }

            if (order.Equals("asc"))
            {
                commentsSorted = redditComments.OrderBy(m => m.Score).ToList();
            }

            return commentsSorted;

        }

        public async Task<List<RedditComment>> GetRedditComments(List<RedditComment> redditComments, List<string> subreddits, int numDays)
        {
            //DateTime today = DateTime.Now.AddDays();
            //DateTime yesterday = DateTime.Now;

            //int dayCounter = 0;

            for (int i = 0; i < numDays; i++)
                foreach (var subreddit in subreddits)
                {
                    var comments = await _psawService.Search<CommentEntry>(new SearchOptions
                    {
                        Subreddit = subreddit,
                        Query = "www.youtube.com/watch", // TODO: seperate out the query for the other link and score
                                                         //Before = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                                                         //After = DateTime.Now.AddDays(i-1).ToString("yyyy-MM-dd"),
                        Before = "2021-05-05",
                        After = "2021-01-01",
                        Size = 5
                    });

                    // reduce day counter so that each time this runs, it goes back one day

                    // the idea now would be to put this into a loop then decrement both today and yesterday by 1 so that it gets the previous day
                    // put this into a service worker?
                    // today.AddDays(-1);
                    // yesterday.AddDays(-1);
                    // then run the search again

                    foreach (var comment in comments)
                    {
                        // check to make sure comment body has a valid YoutubeLinkId in it
                        var validated_link = FindYoutubeId(comment.Body);

                        // if not valid YoutubeLinkId then do not continue
                        if (validated_link.Equals("") || validated_link is null)
                        {
                            break;
                        }

                        // load up RedditComment with data from the API response
                        RedditComment redditComment = new RedditComment
                        {
                            Subreddit = comment.Subreddit,
                            YoutubeLinkId = FindYoutubeId(comment.Body), // use regex to pull youtubeId from comment body
                            CreatedUTC = comment.CreatedUtc,
                            Score = comment.Score,
                            RetrievedUTC = comment.RetrievedOn
                        };

                        // add comment to list of RedditComments
                        redditComments.Add(redditComment);
                    }

                }

            return redditComments;
        }

        // TODO: is this worth having outside the function below?
        const string link_pattern = @"http(?:s?):\/\/(?:www\.)?youtu(?:be\.com\/watch\?v=|\.be\/)([\w\-\]*)(&(amp;)?[\w\?‌​=]*)?";
        Regex link_regex = new Regex(link_pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // should this return multiples? do i need to change youtube linkid to be an array
        // TODO: worried about performance on the regex here?
        public string FindYoutubeId(string commentBody)
        {
            MatchCollection link_matches;
            link_matches = link_regex.Matches(commentBody);

            // TODO: can this ever be null?
            if (link_matches is null)
            {
                return "";
            }

            // TODO: what happens when there is multiple youtube links in a body? is there something to do with that
            foreach (Match match in link_matches)
            {
                if (match is null || match.Equals(""))
                {
                    // TODO: how to get method name in log message?
                    _logger.LogTrace("FindYoutubeId(string commentBody) | invalid link, breaking loop");
                    return "";
                }

                // get the regex groups
                GroupCollection groups = match.Groups;

                if (groups[1].Length < 11) break;

                // trim down id, it should be a maximum of 11 characters
                return (groups[1].Length > 11) ? groups[1].Value.Remove(11) : groups[1].Value;
            }

            // TODO: how to get method name in log message?
            _logger.LogTrace("FindYoutubeId(string commentBody) | invalid link, breaking loop");
            return "";
        }
    }
}
