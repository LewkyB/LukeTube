using LukeTube.Data.Entities;
using LukeTube.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;

namespace LukeTube.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedditController : ControllerBase
    {
        private readonly ISubredditService _subredditService;

        public RedditController(ISubredditService subredditService)
        {
            _subredditService = subredditService;
        }

        // TODO: document in swagger
        [HttpGet]
        [Route("get-all-youtube-ids")]
        public async Task<IReadOnlyList<RedditComment>> GetAllYoutubeIds()
        {
            return await _subredditService.GetAllYoutubeIDs();
        }
        /// <summary>
        /// return json of all links associated with desired chatname
        /// if you enter chatnames, you will receive all chatnames
        /// </summary>
        /// <param name="subredditName"></param>
        /// <returns>JSON list of either all chatnames or chatlinks to associated chatname</returns>
        [HttpGet]
        [Route("subreddit/link-ids/{subredditName}")]
        public Task<IReadOnlyList<string>> GetYoutubeLinkIdsBySubreddit(string subredditName)
        {
            return _subredditService.GetYoutubeLinkIdsBySubreddit(subredditName);
        }

        [HttpGet]
        [Route("subreddit/{subredditName}")]
        public Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subredditName)
        {
            return _subredditService.GetCommentsBySubreddit(subredditName);
        }

        [HttpGet]
        [Route("subreddit-names")]
        public async Task<IReadOnlyList<string>> GetSubredditNames()
        {
            using (var activity = Telemetry.MyActivitySource.StartActivity($"{nameof(GetSubredditNames)}", ActivityKind.Server))
            {
                activity?.SetTag("test", "test");
                activity?.SetTag("test", "test");
                activity?.SetTag("test", "test");
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            Activity.Current?.AddTag("test", "test");

            var tracer = TracerProvider.Default.GetTracer(Telemetry.ServiceName);

            using var parentSpan = tracer.StartActiveSpan("Hi");
            parentSpan.SetAttribute("act test", "act test");
            // parentSpan.
            var act = Activity.Current;
            act?.SetTag("act test", "act test");
            act?.AddEvent(new("act event"));

            return await _subredditService.GetAllSubredditNames();
        }
    }
}
