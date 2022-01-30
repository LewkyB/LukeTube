using luke_site_mvc.Data.Entities;
using luke_site_mvc.Models;
using luke_site_mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                var result = new List<string>();

                var subredditCacheResult = _cache.GetString("cachedSubreddits");

                if (subredditCacheResult is null)
                {
                    result = _pushshiftService.GetSubreddits();

                    var options = new DistributedCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(120));

                    var jsonData = JsonConvert.SerializeObject(result);

                    _cache.SetString("cachedSubreddits", jsonData, options);
                }
                else
                {
                    result = JsonConvert.DeserializeObject<List<string>>(subredditCacheResult);
                }

                var cachedTotalRedditComments = _cache.GetString("cachedTotalRedditComments");

                if (cachedTotalRedditComments is null)
                {
                    int totalRedditComments = _subredditService.GetTotalRedditComments();

                    var options = new DistributedCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(120));

                    var jsonData = JsonConvert.SerializeObject(totalRedditComments);

                    _cache.SetString("cachedTotalRedditComments", jsonData, options);

                    ViewData["TotalLinkCount"] = totalRedditComments;
                    ViewData["IsCachedPage"] = "no";
                }
                else
                {
                    ViewData["IsCachedPage"] = "yes";
                    ViewData["TotalLinkCount"] = JsonConvert.DeserializeObject(cachedTotalRedditComments);
                }

                return Task.FromResult<IActionResult>(View(result));
            }
            catch (Exception ex)
            {
                // TODO: figure out better way to view exceptions, shouldn't the dev page show it when sql throws exception?
                _logger.LogError($"Failed Index():\n {ex}");
                return Task.FromResult<IActionResult>(BadRequest($"Failed Index()\n {ex}"));
            }
        }

        [HttpGet]
        [Route("/Subreddit/{subreddit:alpha}/Links")]
        public IActionResult Links(
            string sortOrder,
            int? pageNumber,
            string subreddit)
        {
            try
            {
                var cachedSubredditLinks = _cache.GetString($"{subreddit}_cachedSubredditLinks");

                var links = new List<RedditComment>();

                if (cachedSubredditLinks is null)
                {
                    links = _subredditService.GetYouLinkIDsBySubreddit(subreddit).ToList();

                    var options = new DistributedCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(120));

                    var jsonData = JsonConvert.SerializeObject(links);

                    _cache.SetString($"{subreddit}_cachedSubredditLinks", jsonData, options);
                    ViewData["LinksIsCached"] = "no";
                }
                else
                {
                    links = JsonConvert.DeserializeObject<List<RedditComment>>(cachedSubredditLinks);
                    ViewData["LinksIsCached"] = "yes";
                }

                ViewData["CurrentSort"] = sortOrder;
                ViewData["LinkSortParm"] = String.IsNullOrEmpty(sortOrder) ? "score_asc" : "";
                ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";


                ViewBag.Title = subreddit;
                ViewBag.count = links.Count;

                switch (sortOrder)
                {
                    case "score_asc":
                        links = links.OrderBy(link => link.Score).ToList();
                        break;
                    case "date":
                        links = links.OrderBy(link => link.CreatedUTC).ToList();
                        break;
                    case "date_desc":
                        links = links.OrderByDescending(link => link.CreatedUTC).ToList();
                        break;
                    default:
                        links = links.OrderByDescending(link => link.Score).ToList();
                        break;
                }

                int pageSize = 4;
                var paginatedList = PaginatedList<RedditComment>.Create(links, pageNumber ?? 1, pageSize);

                return View(paginatedList);
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
