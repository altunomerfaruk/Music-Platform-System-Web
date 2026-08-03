using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicProject.Models.Concrete;
using MusicProject.Models.ViewModels;
using MusicProject.Services.Interface;
using System.Security.Claims;

namespace MusicProject.Controllers
{
    [Authorize(Roles = "Artist")]
    public class ArtistDashboardController : Controller
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly ISongService _songService;
        private readonly IGenreService _genreService;

        public ArtistDashboardController(
            IArtistService artistService,
            IAlbumService albumService,
            ISongService songService,
            IGenreService genreService)
        {
            _artistService = artistService;
            _albumService = albumService;
            _songService = songService;
            _genreService = genreService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            return View(dashboard);
        }

        [HttpGet]
        public IActionResult MySongs()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }
        
            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            { return View("ArtistProfileNotFound"); }

            var songs = _songService.GetSongsByArtistId(dashboard.Artist.Id).ToList();

            var model = new ArtistSongsViewModel
            {
                Artist = dashboard.Artist,
                ArtistInitial = dashboard.ArtistInitial,
                TotalAlbums = dashboard.TotalAlbums,
                TotalSongs = dashboard.TotalSongs,
                Songs = songs,
                TotalStreams = songs.Sum(song => song.SongStat?.TotalStreams ?? 0),
                TotalLikes = songs.Sum(song => song.SongStat?.TotalLikes ?? 0)
            };

            return View(model);

        }

            [HttpGet]
        public IActionResult MyAlbums()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            var albums = _albumService
                .GetAlbumsByArtistId(dashboard.Artist.Id)
                .ToList();

            var model = new ArtistAlbumsViewModel
            {
                Artist = dashboard.Artist,
                ArtistInitial = dashboard.ArtistInitial,
                TotalAlbums = dashboard.TotalAlbums,
                TotalSongs = dashboard.TotalSongs,
                Albums = albums,

                TotalAlbumSongs = albums
                    .Sum(album => album.Songs.Count),

                TotalAlbumStreams = albums
                    .SelectMany(album => album.Songs)
                    .DistinctBy(song => song.Id)
                    .Sum(song => song.SongStat?.TotalStreams ?? 0)
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult CreateAlbum()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            var model = new CreateAlbumViewModel
            {
                Artist = dashboard.Artist,
                ArtistInitial = dashboard.ArtistInitial,
                TotalAlbums = dashboard.TotalAlbums,
                TotalSongs = dashboard.TotalSongs,
                ReleaseDate = DateTime.Today
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult EditSong(int songId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }
 
            var song = _songService.GetArtistSongForEdit(songId,dashboard.Artist.Id);

            if (song == null)
            {
                TempData["ErrorMessage"] ="Şarkı bulunamadı veya bu şarkıyı düzenleme yetkiniz yok.";

                return RedirectToAction(nameof(MySongs));
            }

            var model = new EditSongViewModel
            {
                SongId = song.Id,
                Title = song.Title,
                AlbumId = song.AlbumId,
                LabelId = song.LabelId,

                SelectedGenreIds = song.SongGenres
                    .Select(songGenre => songGenre.GenreId)
                    .ToList()
            };

            FillArtistLayoutData(model, dashboard);

            FillEditSongOptions(
                model,
                dashboard.Artist.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSong(EditSongViewModel model)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            model.SelectedGenreIds ??= new List<int>();

            FillArtistLayoutData(model, dashboard);

            FillEditSongOptions(
                model,
                dashboard.Artist.Id);

            if (model.SelectedGenreIds.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(model.SelectedGenreIds),
                    "En az bir müzik türü seçmelisiniz.");
            }

            if (model.AlbumId.HasValue)
            {
                var selectedAlbum =
                    _albumService.GetArtistAlbumDetails(
                        model.AlbumId.Value,
                        dashboard.Artist.Id);

                if (selectedAlbum == null)
                {
                    ModelState.AddModelError(
                        nameof(model.AlbumId),
                        "Seçilen albüm size ait değil veya bulunamadı.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var song = new Song
            {
                Id = model.SongId,
                Title = model.Title,
                AlbumId = model.AlbumId,
                LabelId = model.LabelId
            };

            try
            {
                _songService.UpdateArtistSong(
                    song,
                    dashboard.Artist.Id,
                    model.SelectedGenreIds);

                TempData["SuccessMessage"] =
                    $"'{model.Title.Trim()}' şarkısı başarıyla güncellendi.";

                return RedirectToAction(nameof(MySongs));
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    exception.Message);

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAlbum(CreateAlbumViewModel model)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            FillArtistLayoutData(model, dashboard);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var album = new Album
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim(),
                CoverImageUrl = string.IsNullOrWhiteSpace(model.CoverImageUrl)
                    ? null
                    : model.CoverImageUrl.Trim(),
                ReleaseDate = model.ReleaseDate,
                ArtistId = dashboard.Artist.Id
            };

            try
            {
                _albumService.AddAlbum(album);
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] =
                $"'{album.Name}' albümü başarıyla oluşturuldu.";

            return RedirectToAction(nameof(MyAlbums));
        }


        [HttpGet]
        public IActionResult CreateSong()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            var model = new CreateSongViewModel();

            FillArtistLayoutData(model, dashboard);
            FillCreateSongOptions(model, dashboard.Artist.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSong(CreateSongViewModel model)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            model.SelectedGenreIds ??= new List<int>();

            FillArtistLayoutData(model, dashboard);
            FillCreateSongOptions(model, dashboard.Artist.Id);

            if (model.SelectedGenreIds.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(model.SelectedGenreIds),
                    "En az bir müzik türü seçmelisiniz.");
            }

            if (model.AlbumId.HasValue)
            {
                var selectedAlbum = _albumService.GetArtistAlbumDetails(
                    model.AlbumId.Value,
                    dashboard.Artist.Id);

                if (selectedAlbum == null)
                {
                    ModelState.AddModelError(
                        nameof(model.AlbumId),
                        "Seçilen albüm bu sanatçı hesabına ait değil.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var song = new Song
            {
                Title = model.Title.Trim(),
                AlbumId = model.AlbumId,
                LabelId = model.LabelId
            };

            try
            {
                _songService.AddSongWithRelations(
                    song,
                    dashboard.Artist.Id,
                    model.SelectedGenreIds);
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] =
                $"'{song.Title}' şarkısı başarıyla eklendi.";

            return RedirectToAction(nameof(MySongs));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSong(int songId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }
            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            try
            {
                _songService.DeleteArtistSong(songId, dashboard.Artist.Id);
                TempData["SuccessMessage"] = "Şarkı başarıyla silindi.";
            }
            catch (InvalidOperationException exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(MySongs));
        }
        private void FillCreateSongOptions(CreateSongViewModel model,int artistId){
            model.AlbumOptions = _albumService
                .GetAlbumsByArtistId(artistId)
                .Select(album => new SelectListItem
                {
                    Value = album.Id.ToString(),
                    Text = album.Name,
                    Selected = model.AlbumId == album.Id
                })
                .ToList();

            model.GenreOptions = _genreService
                .GetAllGenres()
                .Select(genre => new SelectListItem
                {
                    Value = genre.Id.ToString(),
                    Text = genre.Name,
                    Selected = model.SelectedGenreIds.Contains(genre.Id)
                })
                .ToList();
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId);
        }

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Auth");
        }

        private static void FillArtistLayoutData(ArtistLayoutViewModel model,ArtistDashboardViewModel dashboard)
        {
            model.Artist = dashboard.Artist;
            model.ArtistInitial = dashboard.ArtistInitial;
            model.TotalAlbums = dashboard.TotalAlbums;
            model.TotalSongs = dashboard.TotalSongs;
        }

        private void FillEditSongOptions(EditSongViewModel model,int artistId)
        {
            var albums = _albumService
                .GetAlbumsByArtistId(artistId)
                .OrderBy(album => album.Name)
                .ToList();

            var genres = _genreService
                .GetAllGenres()
                .OrderBy(genre => genre.Name)
                .ToList();

            model.AlbumOptions = albums
                .Select(album => new SelectListItem
                {
                    Value = album.Id.ToString(),
                    Text = album.Name,
                    Selected = model.AlbumId == album.Id
                })
                .ToList();

            model.GenreOptions = genres
                .Select(genre => new SelectListItem
                {
                    Value = genre.Id.ToString(),
                    Text = genre.Name,
                    Selected = model.SelectedGenreIds?.Contains(genre.Id) == true
                })
                .ToList();
        }

        [HttpGet]
        public IActionResult ProfileSettings()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            var model = new ArtistProfileSettingsViewModel
            {
                Name = dashboard.Artist.Name,
                Country = dashboard.Artist.Country,
                DebutYear = dashboard.Artist.DebutYear
            };

            FillArtistLayoutData(model, dashboard);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProfileSettings(ArtistProfileSettingsViewModel model)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            FillArtistLayoutData(model, dashboard);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var updated = _artistService.UpdateArtistProfile(userId,model.Name,model.Country,model.DebutYear);

                if (!updated)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Sanatçı profili bulunamadığı için güncelleme yapılamadı.");

                    return View(model);
                }
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(nameof(model.Name), exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] = "Sanatçı profiliniz başarıyla güncellendi.";

            return RedirectToAction(nameof(ProfileSettings));
        }



    }
}