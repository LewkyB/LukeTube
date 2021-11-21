using System;
using System.Diagnostics;
using System.Threading.Tasks;
using luke_site_mvc.Models;
using luke_site_mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Controllers
{
    [Controller]
    public class SubredditController : Controller
    {
        private readonly ISubredditService _chatroomService;
        private readonly ILogger<SubredditController> _logger;
        private readonly IDistributedCache _cache;
        private readonly IPushshiftService _pushshiftService;

        public SubredditController(ISubredditService chatroomService, ILogger<SubredditController> logger, IDistributedCache cache, IPushshiftService pushshiftService)
        {
            _chatroomService = chatroomService;
            _logger = logger;
            _cache = cache;
            _pushshiftService = pushshiftService;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Chatroom.Index() Triggered.");

            try
            {
                var result = await _pushshiftService.GetSubreddits();

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed Index(): {ex}");
                return BadRequest("Failed Index()");
            }
        }

        [HttpGet]
        [Route("/Subreddit/{id:alpha}/Links/{order:alpha}")] 
        public async Task<IActionResult> Links(string id, string order)
        {
            try
            {
                var links = await _pushshiftService.GetLinksFromCommentsAsync(id, order);

                ViewBag.Title = id;
                ViewBag.links = links;

                return View("links", links);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed Links(): {ex}");
                return BadRequest("Links Failed()");
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
