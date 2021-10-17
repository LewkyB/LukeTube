using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using luke_site_mvc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ChatroomsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        private readonly ILogger<ChatroomsController> _logger;
        public ChatroomsController(ILogger<ChatroomsController> logger, IDataRepository dataRepository)
        {
            _logger = logger;
            _logger.LogDebug(1, "NLog injected into ChatroomsController");

            _dataRepository = dataRepository;
        }

        [HttpGet]
        [Produces("application/json")]
        public ActionResult<IEnumerable<Chatroom>> Get()
        {
            try
            {
                return Ok(_dataRepository.GetAllLinksDapper());
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
                    return Ok(_dataRepository.GetAllChatnames());
                
                return Ok(_dataRepository.GetChatLinks(chatname));
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
