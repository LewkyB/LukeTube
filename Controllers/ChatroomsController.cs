using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using luke_site_mvc.Data;
using luke_site_mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ChatroomsController : ControllerBase
    {
        private readonly IChatroomService _chatroomService;
        private readonly ILogger<ChatroomsController> _logger;

        public ChatroomsController(IChatroomService chatroomService, ILogger<ChatroomsController> logger)
        {
            _chatroomService = chatroomService;

            _logger = logger;
            _logger.LogDebug(1, "NLog injected into ChatroomsController");
        }

        [HttpGet]
        [Produces("application/json")]
        public ActionResult<IReadOnlyList<Chatroom>> Get()
        {
            try
            {
                return Ok(_chatroomService.GetAllLinks());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to GetAllLinksDapper()");
                return BadRequest("Failed to GetAllLinksDapper()");
            }
        }
        /// <summary>
        /// return json of all links associated with desired chatname
        /// if you enter chatnames, you will receive all chatnames
        /// </summary>
        /// <param name="chatname"></param>
        /// <returns>JSON list of either all chatnames or chatlinks to associated chatname</returns>
        [HttpGet("{chatname:alpha}")]
        [Produces("application/json")]
        public ActionResult<IReadOnlyList<string>> Get(string chatname)
        {
            try
            {
                // TODO: separate to different functions
                if (chatname.Equals("chatnames")) 
                    return Ok(_chatroomService.GetAllChatNames());
                
                return Ok(_chatroomService.GetChatLinksByChat(chatname));
            }
            catch (Exception ex)
            {
                // TODO: bad logging messages, doesn't reflect actual
                _logger.LogError($"Failed to GetChatLinks()");
                return BadRequest("Failed to GetChatLinks()");
            }
        }
    }
}
