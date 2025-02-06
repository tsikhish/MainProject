using System.Diagnostics;
using System.Security.Claims;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Models.Repository.IRepository;

namespace MvcProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILog _logger;
        private readonly IUserRepository _userRepository;

        public HomeController(ILog logger,IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> GenerateToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) { return NotFound($"{userId} Not Found"); }
            var publicToken=await _userRepository.GenerateTokens(userId);
            ViewData["PublicToken"] = publicToken;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
