using Microsoft.AspNetCore.Mvc;

namespace MusicProject.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}