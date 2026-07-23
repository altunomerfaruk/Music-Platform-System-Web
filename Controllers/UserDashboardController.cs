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
        private readonly IFollowedArtistService _followedArtistService;

        public UserDashboardController(
            ISongService songService,
            IArtistService artistService,
            ILikedSongService likedSongService,
            IFollowedArtistService followedArtistService)
        {
            _songService = songService;
            _artistService = artistService;
            _likedSongService = likedSongService;
            _followedArtistService = followedArtistService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var likedSongIds = _likedSongService
                .GetActiveLikedSongIds(userId)
                .ToHashSet();

            var followedArtistIds = _followedArtistService
                .GetActiveFollowedArtistIds(userId)
                .ToHashSet();

            var model = new UserDashboardViewModel
            {
                Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                PopularSongs = _songService.GetPopularSongs(),
                Artists = _artistService.GetAllArtists(),
                LikedSongIds = likedSongIds,
                FollowedArtistIds = followedArtistIds
            };

            FillLayoutData(model, userId);

            return View(model);
        }



            [HttpGet]
        public IActionResult LikedSongs()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new LikedSongsViewModel
            {
                Songs = _likedSongService
                    .GetLikedSongsByUser(userId)
                    .ToList()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpGet]
        public IActionResult FollowedArtists()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new FollowedArtistsViewModel
            {
                Artists = _followedArtistService
                    .GetFollowedArtistsByUser(userId)
                    .ToList()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        private void FillLayoutData(UserLayoutViewModel model, int userId)
        {
            model.Username = User.FindFirstValue(ClaimTypes.Name) ?? "Kullanıcı";
            model.Role = User.FindFirstValue(ClaimTypes.Role) ?? "User";

            model.LikedSongCount = _likedSongService
                .GetActiveLikedSongIds(userId)
                .Count();

            model.FollowedArtistCount = _followedArtistService
                .GetActiveFollowedArtistIds(userId)
                .Count();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleLike(int songId, string? returnUrl)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (songId <= 0)
            {
                return BadRequest("Geçersiz şarkı bilgisi.");
            }

            _likedSongService.ToggleLike(userId, songId);


            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFollow(int artistId, string? returnUrl)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (artistId <= 0)
            {
                return BadRequest("Geçersiz sanatçı bilgisi.");
            }

            _followedArtistService.ToggleFollow(userId, artistId);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index");
        }


    }
    
}