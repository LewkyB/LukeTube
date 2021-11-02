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
    public class ChatroomController : Controller
    {
        private readonly IChatroomService _chatroomService;
        private readonly ILogger<ChatroomController> _logger;
        private readonly IDistributedCache _cache;

        public ChatroomController(IChatroomService chatroomService, ILogger<ChatroomController> logger, IDistributedCache cache)
        {
            _chatroomService = chatroomService;
            _logger = logger;
            _cache = cache;
        }

        //public IActionResult Index()
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Chatroom.Index() Triggered.");

            try
            {
                var result = await _chatroomService.GetAllChatNames();

                return View(result);
            }
            catch (Exception ex)
            {
                // TODO: better log messages
                _logger.LogError($"Failed Index(): {ex}");
                return BadRequest("Failed Index()");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Links(string id)
        {
            try
            {
                // TODO: is this a safe way to pass sql parameter?
                var links = await _chatroomService.GetChatLinksByChat(id);

                ViewBag.Title = id;

                return View(links);
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
                // TODO: better log messages
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
