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
        private readonly ILikedSongService _likedSongService;

        public UserDashboardController(
            ISongService songService,
            IArtistService artistService,
            ILikedSongService likedSongService)
        {
            _songService = songService;
            _artistService = artistService;
            _likedSongService = likedSongService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
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

                PopularSongs =
                    _songService.GetPopularSongs(),

                Artists =
                    _artistService.GetAllArtists(),

                LikedSongIds =
                    _likedSongService
                        .GetActiveLikedSongIds(userId)
                        .ToHashSet()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleLike(int songId)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return RedirectToAction(
                    "Login",
                    "Auth"
                );
            }

            if (songId <= 0)
            {
                return BadRequest(
                    "Geçersiz şarkı bilgisi."
                );
            }

            _likedSongService.ToggleLike(
                userId,
                songId
            );

            return RedirectToAction("Index");
        }
    }
}