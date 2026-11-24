
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ThirdSpace.Models;
using ThirdSpace.Interfaces;

namespace ThirdSpace.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomePostRepository _homePostRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IHomePostRepository homePostRepository)
        {
            _logger = logger;
            _homePostRepository = homePostRepository;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _homePostRepository.GetAllAsync();

            ViewBag.HomePosts = posts;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}
