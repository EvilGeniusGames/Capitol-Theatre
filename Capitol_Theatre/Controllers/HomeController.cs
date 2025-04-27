using System.Diagnostics;
using Capitol_Theatre.Models;
using Microsoft.AspNetCore.Mvc;

namespace Capitol_Theatre.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Listing", "Movies", new { mode = "NowShowing" });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("portal-access")]
        public IActionResult AdminLoginRedirect()
        {
            return Redirect("/Identity/Account/Login");
        }


    }
}
