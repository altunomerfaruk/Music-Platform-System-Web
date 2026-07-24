using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Controllers.Base; 
using MusicProject.Models.ViewModels;
using MusicProject.Services.Interface;
using System.Security.Claims;

namespace MusicProject.Controllers
{
    [Authorize(Roles = "User,Artist")]
    public class UserDashboardController : UserBaseController
    {

        private readonly ISongService _songService;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly ILikedSongService _likedSongService;
        private readonly IFollowedArtistService _followedArtistService;

        public UserDashboardController(
            ISongService songService,
            IArtistService artistService,
            IAlbumService albumService,
            ILikedSongService likedSongService,
            IFollowedArtistService followedArtistService)
        {
            _songService = songService;
            _artistService = artistService;
            _albumService = albumService;
            _likedSongService = likedSongService;
            _followedArtistService = followedArtistService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
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
        public IActionResult SongDetails(int songId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            if (songId <= 0)
            {
                return BadRequest("Geçersiz şarkı bilgisi.");
            }

            var song = _songService.GetSongDetails(songId);

            if (song == null)
            {
                return NotFound("Şarkı bulunamadı.");
            }

            var likedSongIds = _likedSongService
                .GetActiveLikedSongIds(userId)
                .ToHashSet();

            var model = new SongDetailsViewModel
            {
                Song = song,
                IsLiked = likedSongIds.Contains(songId)
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpGet]
        public IActionResult LikedSongs()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
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
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
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

        [HttpGet]
        public IActionResult ArtistDetails(int artistId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            if (artistId <= 0)
            {
                return BadRequest("Geçersiz sanatçı bilgisi.");
            }

            var artist = _artistService.GetArtistDetails(artistId);

            if (artist == null)
            {
                return NotFound("Sanatçı bulunamadı.");
            }

            var followedArtistIds = _followedArtistService
                .GetActiveFollowedArtistIds(userId)
                .ToHashSet();

            var model = new ArtistDetailsViewModel
            {
                Artist = artist,
                IsFollowed = followedArtistIds.Contains(artistId),
                LikedSongIds = _likedSongService
                    .GetActiveLikedSongIds(userId)
                    .ToHashSet()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpGet]
        public IActionResult AlbumDetails(int albumId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            if (albumId <= 0)
            {
                return BadRequest("Geçersiz albüm bilgisi.");
            }

            var album = _albumService.GetAlbumDetails(albumId);

            if (album == null)
            {
                return NotFound("Albüm bulunamadı.");
            }

            var model = new AlbumDetailsViewModel
            {
                Album = album,
                LikedSongIds = _likedSongService
                    .GetActiveLikedSongIds(userId)
                    .ToHashSet()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpGet]
        public IActionResult AllSongs()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var model = new AllSongsViewModel
            {
                Songs = _songService.GetSongsSortedByAlphabet(),
                LikedSongIds = _likedSongService
                    .GetActiveLikedSongIds(userId)
                    .ToHashSet()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleLike(int songId, string? returnUrl)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
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

            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
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
    }
}