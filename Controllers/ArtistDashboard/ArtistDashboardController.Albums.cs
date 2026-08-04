using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.Concrete;
using MusicProject.ViewModels.ArtistDashboard;

namespace MusicProject.Controllers
{
    // Sanatcinin kendi albumleri: listeleme ve olusturma.
    public partial class ArtistDashboardController
    {
        [HttpGet]
        public IActionResult MyAlbums()
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
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
                TotalAlbumSongs = albums.Sum(album => album.Songs.Count),
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
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAlbum(CreateAlbumViewModel model)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
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
                ModelState.AddModelError(nameof(model.Name), exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] =
                $"'{album.Name}' albümü başarıyla oluşturuldu.";

            return RedirectToAction(nameof(MyAlbums));
        }
    }
}
