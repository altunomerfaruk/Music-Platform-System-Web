using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Services.Interface;
using System.Security.Claims;

namespace MusicProject.Controllers
{
    [Authorize(Roles = "Artist")]
    public class ArtistDashboardController : Controller
    {
        private readonly IArtistService _artistService;

        public ArtistDashboardController(IArtistService artistService)
        {
            _artistService = artistService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model =
                _artistService.GetArtistDashboard(userId);

            if (model == null)
            {
                return View("ArtistProfileNotFound");
            }

            return View(model);
        }
    }
}