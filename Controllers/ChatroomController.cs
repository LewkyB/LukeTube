using System;
using luke_site_mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Controllers
{
    public class ChatroomController : Controller
    {
        private readonly IChatroomService _chatroomService;
        private readonly ILogger<ChatroomController> _logger;
        public ChatroomController(IChatroomService chatroomService, ILogger<ChatroomController> logger)
        {
            _chatroomService = chatroomService;

            _logger = logger;
        }

        [HttpGet]
        public IActionResult Links(string id)
        {
            try
            {
                // TODO: is this a safe way to pass sql parameter?
                // TODO: how to deal with bad characters that trigger IIS?
                var links = _chatroomService.GetChatLinksByChat(id);
                return View(links);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed Links(): {ex}");
                return BadRequest("Links Failed()");
            }

        }
    }
}
