using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using luke_site_mvc.Data;
using luke_site_mvc.Models;
using luke_site_mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IChatroomService _chatroomService;

        public HomeController(IChatroomService chatroomService, ILogger<HomeController> logger)
        {
            _chatroomService = chatroomService;

            _logger = logger;
            _logger.LogDebug(1, "NLog injected into HomeController");
        }

        public IActionResult Index()
        {
            _logger.LogInformation("HomeController.Index() Triggered.");

            try
            {
                var result = _chatroomService.GetAllChatNames();
                return View(result);
            }
            catch (Exception ex)
            {
                // TODO: better log messages
                _logger.LogError($"Failed Index(): {ex}");
                return BadRequest("Failed Index()");
            }
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("HomeController.Privacy() Triggered.");

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
