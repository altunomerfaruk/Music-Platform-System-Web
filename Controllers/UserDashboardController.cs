using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Controllers.Base;
using MusicProject.Models.Enums;
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
        private readonly IUserService _userService;
        private readonly IGenreService _genreService;

        public UserDashboardController(
            ISongService songService,
            IArtistService artistService,
            IAlbumService albumService,
            ILikedSongService likedSongService,
            IFollowedArtistService followedArtistService,
            IListeningHistoryService listeningHistoryService,
            IUserService userService,
            IGenreService genreService)
        {
            _songService = songService;
            _artistService = artistService;
            _albumService = albumService;
            _likedSongService = likedSongService;
            _followedArtistService = followedArtistService;
            _listeningHistoryService = listeningHistoryService;
            _userService = userService;
            _genreService = genreService;
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
        [HttpGet]
        public IActionResult AllArtists(string? search, string? country, string? sort, bool followedOnly = false)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            search = search?.Trim() ?? string.Empty;
            country = country?.Trim() ?? string.Empty;
            sort = string.IsNullOrWhiteSpace(sort) ? "name-asc" : sort;

            var allArtists = _artistService
                .GetAllArtists()
                .ToList();

            var followedArtistIds = _followedArtistService
                .GetActiveFollowedArtistIds(userId)
                .ToHashSet();

            var countries = allArtists
                .Where(artist => !string.IsNullOrWhiteSpace(artist.Country))
                .Select(artist => artist.Country!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(countryName => countryName)
                .ToList();

            IEnumerable<MusicProject.Models.Concrete.Artist> filteredArtists = allArtists;

            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredArtists = filteredArtists.Where(artist =>
                    artist.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                filteredArtists = filteredArtists.Where(artist =>
                    !string.IsNullOrWhiteSpace(artist.Country) &&
                    artist.Country.Equals(country, StringComparison.OrdinalIgnoreCase));
            }

            if (followedOnly)
            {
                filteredArtists = filteredArtists.Where(artist =>
                    followedArtistIds.Contains(artist.Id));
            }

            filteredArtists = sort switch
            {
                "name-desc" => filteredArtists.OrderByDescending(artist => artist.Name),

                "year-newest" => filteredArtists
                    .OrderByDescending(artist => artist.DebutYear.HasValue)
                    .ThenByDescending(artist => artist.DebutYear)
                    .ThenBy(artist => artist.Name),

                "year-oldest" => filteredArtists
                    .OrderByDescending(artist => artist.DebutYear.HasValue)
                    .ThenBy(artist => artist.DebutYear)
                    .ThenBy(artist => artist.Name),

                _ => filteredArtists.OrderBy(artist => artist.Name)
            };

            var model = new AllArtistsViewModel
            {
                Artists = filteredArtists.ToList(),
                FollowedArtistIds = followedArtistIds,
                Search = search,
                Country = country,
                Sort = sort,
                FollowedOnly = followedOnly,
                Countries = countries,
                TotalArtistCount = allArtists.Count
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
        public IActionResult AllGenres()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var model = new AllGenresViewModel
            {
                Genres = _genreService
                    .GetAllGenres()
                    .OrderBy(genre => genre.Name)
                    .ToList()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        // DEĞİŞİKLİK:
        // Seçilen müzik türünü ve o türe bağlı şarkıları gösterir.
        [HttpGet]
        public IActionResult GenreDetails(int genreId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            if (genreId <= 0)
            {
                return BadRequest("Geçersiz tür bilgisi.");
            }

            var genre = _genreService.GetGenreDetails(genreId);

            if (genre == null)
            {
                return NotFound("Tür bulunamadı.");
            }

            var model = new GenreDetailsViewModel
            {
                Genre = genre,

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
        public IActionResult UserSettings()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var model = _userService.GetUserSettings(userId);

            if (model == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserSettings(UserSettingsViewModel model)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            model.UserId = userId;

            if (!ModelState.IsValid)
            {
                FillLayoutData(model, userId);
                return View(model);
            }

            var result = _userService.UpdateUserSettings(userId, model);

            switch (result)
            {
                case UserSettingsResult.Success:
                    await RefreshUserClaimsAsync(userId, model);

                    TempData["SuccessMessage"] =
                        "Hesap bilgileriniz başarıyla güncellendi.";

                    return RedirectToAction(nameof(UserSettings));

                case UserSettingsResult.UsernameAlreadyExists:
                    ModelState.AddModelError(
                        nameof(model.Username),
                        "Bu kullanıcı adı başka bir kullanıcı tarafından kullanılıyor."
                    );
                    break;

                case UserSettingsResult.EmailAlreadyExists:
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor."
                    );
                    break;

                case UserSettingsResult.CurrentPasswordIncorrect:
                    ModelState.AddModelError(
                        nameof(model.CurrentPassword),
                        "Mevcut şifreniz yanlış."
                    );
                    break;

                case UserSettingsResult.NewPasswordRequired:
                    ModelState.AddModelError(
                        nameof(model.NewPassword),
                        "Şifre değiştirmek için yeni şifrenizi girmelisiniz."
                    );
                    break;

                case UserSettingsResult.UserNotFound:
                    return NotFound("Kullanıcı bulunamadı.");

                default:
                    ModelState.AddModelError(
                        string.Empty,
                        "Hesap bilgileri güncellenirken beklenmeyen bir hata oluştu."
                    );
                    break;
            }

            FillLayoutData(model, userId);

            return View(model);
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

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
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

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
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

        private async Task RefreshUserClaimsAsync(int userId, UserSettingsViewModel model)
        {
            var currentRole =
                User.FindFirstValue(ClaimTypes.Role) ?? "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Role, currentRole)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var claimsPrincipal =
                new ClaimsPrincipal(claimsIdentity);

            var authenticationResult =
                await HttpContext.AuthenticateAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

            var authenticationProperties =
                authenticationResult.Properties ??
                new AuthenticationProperties();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authenticationProperties
            );
        }
    }
}