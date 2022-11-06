using LukeTube.Services;
using LukeTube.Models.Pushshift;
using Microsoft.AspNetCore.Mvc;

namespace LukeTube.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class PushshiftController : ControllerBase
    {
        private readonly IPushshiftService _pushshiftService;

        public PushshiftController(IPushshiftService pushshiftService)
        {
            _pushshiftService = pushshiftService;
        }

        [HttpGet]
        [Route("get-all-reddit-comments")]
        public async Task<IReadOnlyList<RedditComment>> GetAllRedditComments()
        {
            return await _pushshiftService.GetAllRedditComments();
        }

        [HttpGet]
        [Route("subreddit/link-ids/{subredditName}")]
        public Task<IReadOnlyList<string>> GetYoutubeLinkIdsBySubreddit(string subredditName)
        {
            return _pushshiftService.GetYoutubeLinkIdsBySubreddit(subredditName);
        }

        [HttpGet]
        [Route("subreddit/{subredditName}")]
        public Task<IReadOnlyList<RedditComment>> GetCommentsBySubreddit(string subredditName)
        {
            return _pushshiftService.GetCommentsBySubreddit(subredditName);
        }

        [HttpGet]
        [Route("subreddit-names")]
        public async Task<IReadOnlyList<string>> GetSubredditNames()
        {
            return await _pushshiftService.GetAllSubredditNames();
        }
    }
}
