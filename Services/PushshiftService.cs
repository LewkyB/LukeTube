﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using luke_site_mvc.Data;
using luke_site_mvc.Data.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PsawSharp;
using PsawSharp.Entries;
using PsawSharp.Requests.Options;

namespace luke_site_mvc.Services
{
    public interface IPushshiftService
    {
        string FindYoutubeId(string commentBody);
        Task<List<RedditComment>> GetLinksFromCommentsAsync(string selected_subreddit);
        Task<List<string>> GetSubreddits();
    }
    public class PushshiftService : IPushshiftService
    {
        private readonly ILogger<PushshiftService> _logger;
        private readonly IDistributedCache _cache;
        private readonly ChatroomContext _chatroomContext;

        public PushshiftService(ILogger<PushshiftService> logger, IDistributedCache distributedCache, ChatroomContext chatroomContext)
        {
            _logger = logger;
            _cache = distributedCache;
            _chatroomContext = chatroomContext;
        }
        // TODO: not actually async
        public async Task<List<string>> GetSubreddits()
        {
            // TODO: provide a better list of subreddits
            List<string> subreddits = new List<string>()
            {
                "space",
                "science",
                "mealtimevideos",
                "woodworking"
            };

            return subreddits;
        }

        public async Task<List<RedditComment>> GetLinksFromCommentsAsync(string selected_subreddit)
        {

            List<RedditComment> redditComments = new List<RedditComment>();

            var client = new PsawClient();
            var comments = await client.Search<CommentEntry>(new SearchOptions
            {
                Subreddit = selected_subreddit,
                Query = "www.youtube.com/watch?v=", // TODO: are there any types of youtube links with 11 char ids
                Size = 100
            });

            foreach (var comment in comments)
            {
                var validated_link = FindYoutubeId(comment.Body);

                // if the link is empty do not include it
                if (validated_link.Equals("") || validated_link is null)
                {
                    _logger.LogDebug("invalid link, breaking loop");
                    break;
                }

                RedditComment redditComment = new RedditComment
                {
                    Subreddit = comment.Subreddit,
                    YoutubeLinkId = FindYoutubeId(comment.Body),
                    CommentLink = comment.LinkId, // TODO: not sure if this is what i think it is
                    CreatedUTC = comment.CreatedUtc,
                    Score = comment.Score,
                    RetrievedUTC = comment.RetrievedOn
                };

                redditComments.Add(redditComment);
            }

            // load up the database
            // TODO: need to prevent duplicate entries
            // going to a link, then back, then back to that link, will make 2 entries
            await _chatroomContext.AddRangeAsync(redditComments);
            await _chatroomContext.SaveChangesAsync();

            // sort comments so that the highest scored video shows at the top
            List<RedditComment> commentsSortedByScoreDesc = redditComments.OrderByDescending(m => m.Score).ToList();

            return commentsSortedByScoreDesc;
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
                    return "";
                }

                // get the regex groups
                GroupCollection groups = match.Groups;

                // trim down id, it should be a maximum of 11 characters
                return (groups[1].Length > 11) ? groups[1].Value.Remove(11) : groups[1].Value;
            }

            return "";
        }
    }
}
