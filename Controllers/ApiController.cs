using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using luke_site_mvc.Data.Entities;
using luke_site_mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ApiController : ControllerBase
    {
        private readonly ISubredditService _subredditService;
        private readonly ILogger<ApiController> _logger;

        public ApiController(ISubredditService subredditService, ILogger<ApiController> logger)
        {
            _subredditService = subredditService;
            _logger = logger;
        }

        // TODO: document in swagger
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IReadOnlyList<RedditComment>>> Get()
        {
            _logger.LogInformation("ChatroomsController.Get()");

            try
            {
                return Ok(await _subredditService.GetAllYoutubeIDs());
            }
            catch (Exception ex)
            {
                // TODO: better log messages
                _logger.LogError($"Failed to GetAllLinks(): {ex}");
                return BadRequest("Failed to GetAllLinks()");
            }
        }
        /// <summary>
        /// return json of all links associated with desired chatname
        /// if you enter chatnames, you will receive all chatnames
        /// </summary>
        /// <param name="subredditName"></param>
        /// <returns>JSON list of either all chatnames or chatlinks to associated chatname</returns>
        [HttpGet("{subredditName:alpha}")]
        [Produces("application/json")]
        // TODO: change this overloaded Get to make overloading not required
        public async Task<ActionResult<IReadOnlyList<string>>> Get(string subredditName)
        {
            _logger.LogInformation("ChatroomsController.Get(string chatname) Triggered.");

            try
            {
                // TODO: separate to different functions
                if (subredditName.Equals("subredditnames"))
                    return Ok(await _subredditService.GetAllSubredditNames());

                return Ok(await _subredditService.GetYouLinkIDsBySubreddit(subredditName));
            }
            catch (Exception ex)
            {
                // TODO: bad logging messages, doesn't reflect actual
                _logger.LogError($"Failed to GetChatLinks(): {ex}");
                return BadRequest("Failed to GetChatLinks()");
            }
        }
    }
}
