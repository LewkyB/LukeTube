using LukeTube.Services;
using LukeTubeLib.Models.Pushshift;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace LukeTube.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class PushshiftController : ControllerBase
    {
        private readonly IPushshiftService _pushshiftService;

        public PushshiftController(IPushshiftService pushshiftService)
        {
            _pushshiftService = pushshiftService;
        }

        [HttpGet]
        [Route("get-all-reddit-comments")]
        public Task<IReadOnlyList<RedditComment>> GetAllRedditComments()
        {
            return _pushshiftService.GetAllRedditComments();
        }

        [HttpGet]
        [Route("subreddit/link-ids/{subredditName}")]
        public Task<IReadOnlyList<string>> GetYoutubeLinkIdsBySubreddit(string subredditName)
        {
            return _pushshiftService.GetYoutubeLinkIdsBySubreddit(subredditName);
        }

        [HttpGet]
        [Route("subreddit/{subredditName}")]
        public Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit([FromQuery] string subredditName)
        {
            return _pushshiftService.GetCommentsBySubreddit(subredditName);
        }

        [HttpGet]
        [Route("subreddit-names")]
        public Task<IReadOnlyList<string>> GetSubredditNames()
        {
            return _pushshiftService.GetAllSubredditNames();
        }

        [HttpGet]
        [Route("subreddit-names-with-link-count")]
        [OutputCache(NoStore = true)]
        public Task<IReadOnlyList<SubredditWithCount>> GetSubredditNamesWithLinkCount()
        {
            return _pushshiftService.GetSubredditsWithLinkCounts();
        }

        [HttpGet]
        [Route("{subredditName}/paged-comments/{pageNumber}")]
        public Task<IReadOnlyList<RedditComment>> GetPagedRedditCommentsBySubreddit(string subredditName,int pageNumber)
        {
            return _pushshiftService.GetPagedRedditCommentsBySubreddit(subredditName, pageNumber);
        }

        [HttpGet]
        [Route("youtube-video-total-count")]
        public Task<int> GetTotalYoutubeIdCount()
        {
            return _pushshiftService.GetTotalRedditComments();
        }
    }
}