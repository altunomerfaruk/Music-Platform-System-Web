using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Controllers.Base;
using MusicProject.Services.Interface;
using MusicProject.ViewModels.UserDashboard;
using System.Security.Claims;

namespace MusicProject.Controllers
{
    [Authorize(Roles = "User,Artist")]
    public partial class UserDashboardController : DashboardControllerBase
    {
        private const int FeaturedArtistCount = 6;

        private readonly ISongService _songService;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly ILikedSongService _likedSongService;
        private readonly IFollowedArtistService _followedArtistService;
        private readonly IListeningHistoryService _listeningHistoryService;
        private readonly IUserService _userService;
        private readonly IGenreService _genreService;
        private readonly IAudioStorageService _audioStorageService;

        public UserDashboardController(
            ISongService songService,
            IArtistService artistService,
            IAlbumService albumService,
            ILikedSongService likedSongService,
            IFollowedArtistService followedArtistService,
            IListeningHistoryService listeningHistoryService,
            IUserService userService,
            IGenreService genreService,
            IAudioStorageService audioStorageService)
        {
            _songService = songService;
            _artistService = artistService;
            _albumService = albumService;
            _likedSongService = likedSongService;
            _followedArtistService = followedArtistService;
            _listeningHistoryService = listeningHistoryService;
            _userService = userService;
            _genreService = genreService;
            _audioStorageService = audioStorageService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var model = new UserDashboardViewModel
            {
                PopularSongs = _songService
                    .GetPopularSongs()
                    .Select(ToSongListItem)
                    .ToList(),

                Artists = _artistService
                    .GetFeaturedArtists(FeaturedArtistCount)
                    .Select(ToArtistListItem)
                    .ToList(),

                LikedSongIds = _likedSongService
                    .GetActiveLikedSongIds(userId)
                    .ToHashSet(),

                FollowedArtistIds = _followedArtistService
                    .GetActiveFollowedArtistIds(userId)
                    .ToHashSet(),

                TotalListeningCount = _listeningHistoryService
                    .GetTotalListeningCount(userId),

                RecentListeningHistory = _listeningHistoryService
                    .GetRecentListeningHistory(userId, 5)
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        private void FillLayoutData(UserLayoutViewModel model, int userId)
        {
            model.Username =
                User.FindFirstValue(ClaimTypes.Name) ?? "Kullanıcı";

            model.Role =
                User.FindFirstValue(ClaimTypes.Role) ?? "User";

            model.LikedSongCount = _likedSongService
                .GetActiveLikedSongIds(userId)
                .Count();

            model.FollowedArtistCount = _followedArtistService
                .GetActiveFollowedArtistIds(userId)
                .Count();
        }

        private IActionResult RedirectBack(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}