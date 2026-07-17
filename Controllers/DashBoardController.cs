using Microsoft.AspNetCore.Mvc;

namespace MusicProject.Controllers
{
    public class DashBoardController : Controller
    {
        [HttpGet]
        public IActionResult User()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Artist()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Admin()
        {
            return View();
        }
    }
}