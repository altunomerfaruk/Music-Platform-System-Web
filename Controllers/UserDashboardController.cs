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
        private readonly IListeningHistoryService _listeningHistoryService;
        public UserDashboardController(
            ISongService songService,
            IArtistService artistService,
            IAlbumService albumService,
            ILikedSongService likedSongService,
            IFollowedArtistService followedArtistService,
            IListeningHistoryService listeningHistoryService)
        {
            _songService = songService;
            _artistService = artistService;
            _albumService = albumService;
            _likedSongService = likedSongService;
            _followedArtistService = followedArtistService;
            _listeningHistoryService = listeningHistoryService;
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
                PopularSongs = _songService.GetPopularSongs(),

                Artists = _artistService
                    .GetAllArtists()
                    .Take(6)
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
        public IActionResult AllArtists()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var model = new AllArtistsViewModel
            {
                Artists = _artistService
                    .GetAllArtists()
                    .OrderBy(artist => artist.Name)
                    .ToList(),

                FollowedArtistIds = _followedArtistService
                    .GetActiveFollowedArtistIds(userId)
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

        [HttpGet]
        public IActionResult Search(string? query)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            query = query?.Trim() ?? string.Empty;

            var model = new SearchResultsViewModel
            {
                Query = query,
                LikedSongIds = _likedSongService
                    .GetActiveLikedSongIds(userId)
                    .ToHashSet(),

                FollowedArtistIds = _followedArtistService
                    .GetActiveFollowedArtistIds(userId)
                    .ToHashSet()
            };

            if (!string.IsNullOrWhiteSpace(query))
            {
                model.Songs = _songService
                    .GetSongsSortedByAlphabet()
                    .Where(song =>
                        song.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (song.Album != null &&
                         song.Album.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                model.Artists = _artistService
                    .GetAllArtists()
                    .Where(artist =>
                        artist.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrWhiteSpace(artist.Country) &&
                         artist.Country.Contains(query, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(artist => artist.Name)
                    .ToList();

                model.Albums = _albumService
                    .GetAllAlbums()
                    .Where(album =>
                        album.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        album.Artist.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(album => album.Name)
                    .ToList();
            }

            FillLayoutData(model, userId);

            return View("SearchResults", model);
        }
        [HttpGet]
        public IActionResult SearchSuggestions(string? query)
        {
            query = query?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new
                {
                    songs = Array.Empty<object>(),
                    artists = Array.Empty<object>(),
                    albums = Array.Empty<object>()
                });
            }

            var songs = _songService
                .GetSongsSortedByAlphabet()
                .Where(song =>
                    song.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (song.Album != null &&
                     song.Album.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .Take(4)
                .Select(song => new
                {
                    id = song.Id,
                    name = song.Title,
                    subtitle = song.Album?.Name ?? "Single",
                    type = "song"
                })
                .ToList();

            var artists = _artistService
                .GetAllArtists()
                .Where(artist =>
                    artist.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(artist => artist.Name)
                .Take(4)
                .Select(artist => new
                {
                    id = artist.Id,
                    name = artist.Name,
                    subtitle = "Sanatçı",
                    type = "artist"
                })
                .ToList();

            var albums = _albumService
                .GetAllAlbums()
                .Where(album =>
                    album.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    album.Artist.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(album => album.Name)
                .Take(4)
                .Select(album => new
                {
                    id = album.Id,
                    name = album.Name,
                    subtitle = album.Artist.Name,
                    type = "album"
                })
                .ToList();

            return Json(new
            {
                songs,
                artists,
                albums
            });
        }
        [HttpGet]
        public IActionResult ListeningHistory()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var model = new ListeningHistoryViewModel
            {
                ListeningHistory = _listeningHistoryService
                    .GetRecentListeningHistory(userId, 100),

                TotalListeningCount = _listeningHistoryService
                    .GetTotalListeningCount(userId)
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaySong(int songId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Kullanıcı oturumu bulunamadı."
                });
            }

            var isAdded = _listeningHistoryService.AddListening(userId, songId);

            if (!isAdded)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Şarkı bulunamadı."
                });
            }

            return Json(new
            {
                success = true,
                message = "Dinleme kaydı oluşturuldu."
            });
        }
    }
}