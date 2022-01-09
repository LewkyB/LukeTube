using luke_site_mvc.Models;
using luke_site_mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace luke_site_mvc.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)] // prevent this controller from showing up on swagger
    [Controller]
    public class SubredditController : Controller
    {
        private readonly ISubredditService _subredditService;
        private readonly ILogger<SubredditController> _logger;
        private readonly IDistributedCache _cache;
        private readonly IPushshiftService _pushshiftService;

        public SubredditController(ISubredditService subredditService, ILogger<SubredditController> logger, IDistributedCache cache, IPushshiftService pushshiftService)
        {
            _subredditService = subredditService;
            _logger = logger;
            _cache = cache;
            _pushshiftService = pushshiftService;
        }

        public Task<IActionResult> Index()
        {
            _logger.LogInformation("Chatroom.Index() Triggered.");

            try
            {
                var result = _pushshiftService.GetSubreddits();

                return Task.FromResult<IActionResult>(View(result));
            }
            catch (Exception ex)
            {
                // TODO: figure out better way to view exceptions, shouldn't the dev page show it when sql throws exception? 
                _logger.LogError($"Failed Index():\n {ex}");
                return Task.FromResult<IActionResult>(BadRequest($"Failed Index()\n {ex}"));
            }
        }
        
        // TODO: fix url for this page, order is no longer required
        [HttpGet]
        [Route("/Subreddit/{id:alpha}/Links/{order:alpha}")]
        public async Task<IActionResult> Links(string id, string order)
        {
            try
            {
                var links = await _subredditService.GetYouLinkIDsBySubreddit(id);

                var sortedLinks =

                ViewBag.Title = id;
                ViewBag.links = links;

                return View("links", links);
            }
            catch (Exception ex)
            {
                // TODO: figure out better way to view exceptions, shouldn't the dev page show it when sql throws exception? 
                _logger.LogError($"Failed Links():\n {ex}");
                return BadRequest($"Links Failed()\n {ex}");
            }

        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("Chatroom.Privacy() Triggered.");

            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed Privacy(): {ex}");
                return BadRequest("Failed Privary()");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
