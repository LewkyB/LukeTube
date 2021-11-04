using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using luke_site_mvc.Data;
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
        private readonly IChatroomService _chatroomService;
        private readonly ILogger<ApiController> _logger;

        public ApiController(IChatroomService chatroomService, ILogger<ApiController> logger)
        {
            _chatroomService = chatroomService;
            _logger = logger;
        }

        // TODO: document in swagger
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IReadOnlyList<Chatroom>>> Get()
        {
            _logger.LogInformation("ChatroomsController.Get()");

            try
            {
                return Ok(await _chatroomService.GetAllLinks());
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
        /// <param name="chatname"></param>
        /// <returns>JSON list of either all chatnames or chatlinks to associated chatname</returns>
        [HttpGet("{chatname:alpha}")]
        [Produces("application/json")]
        // TODO: change this overloaded Get to make overloading not required
        public async Task<ActionResult<IReadOnlyList<string>>> Get(string chatname)
        {
            _logger.LogInformation("ChatroomsController.Get(string chatname) Triggered.");

            try
            {
                // TODO: separate to different functions
                if (chatname.Equals("chatnames"))
                    return Ok(await _chatroomService.GetAllChatNames());

                return Ok(await _chatroomService.GetChatLinksByChat(chatname));
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
