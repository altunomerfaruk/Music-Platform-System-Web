using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.ViewModels;
using MusicProject.Services.Interface;
using System.Security.Claims;

namespace MusicProject.Controllers
{
    [Authorize(Roles = "User,Artist")]
    public class UserDashboardController : Controller
    {
        private readonly ISongService _songService;
        private readonly IArtistService _artistService;

        public UserDashboardController(
            ISongService songService,
            IArtistService artistService)
        {
            _songService = songService;
            _artistService = artistService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdValue, out _))
            {
                return RedirectToAction(
                    "Login",
                    "Auth"
                );
            }

            var model = new UserDashboardViewModel
            {
                Username = User.FindFirstValue(
                    ClaimTypes.Name
                ) ?? "Kullanıcı",

                Email = User.FindFirstValue(
                    ClaimTypes.Email
                ) ?? string.Empty,

                Role = User.FindFirstValue(
                    ClaimTypes.Role
                ) ?? "User",

                PopularSongs = _songService.GetPopularSongs(),

                Artists = _artistService.GetAllArtists()
            };

            return View(model);
        }
    }
}