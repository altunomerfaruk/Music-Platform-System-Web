using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.Concrete;
using MusicProject.ViewModels.ArtistDashboard;

namespace MusicProject.Controllers
{
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

        [HttpGet]
        public IActionResult EditAlbum(int albumId)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            var album = _albumService.GetArtistAlbumDetails(albumId, dashboard.Artist.Id);

            if (album == null)
            {
                TempData["ErrorMessage"] =
                    "Albüm bulunamadı veya bu albümü düzenleme yetkiniz yok.";

                return RedirectToAction(nameof(MyAlbums));
            }

            var model = new EditAlbumViewModel
            {
                AlbumId = album.Id,
                Name = album.Name,
                Description = album.Description,
                CoverImageUrl = album.CoverImageUrl,
                ReleaseDate = album.ReleaseDate
            };

            FillArtistLayoutData(model, dashboard);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAlbum(EditAlbumViewModel model)
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

            try
            {
                var updated = _albumService.UpdateArtistAlbum(
                    model.AlbumId,
                    dashboard.Artist.Id,
                    model.Name,
                    model.Description,
                    model.CoverImageUrl,
                    model.ReleaseDate);

                if (!updated)
                {
                    TempData["ErrorMessage"] =
                        "Albüm bulunamadı veya bu albümü düzenleme yetkiniz yok.";

                    return RedirectToAction(nameof(MyAlbums));
                }
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(nameof(model.Name), exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] =
                $"'{model.Name.Trim()}' albümü başarıyla güncellendi.";

            return RedirectToAction(nameof(MyAlbums));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAlbum(int albumId)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            try
            {
                var deleted = _albumService.DeleteArtistAlbum(albumId, dashboard.Artist.Id);

                if (!deleted)
                {
                    TempData["ErrorMessage"] =
                        "Albüm bulunamadı veya bu albümü silme yetkiniz yok.";

                    return RedirectToAction(nameof(MyAlbums));
                }

                TempData["SuccessMessage"] = "Albüm başarıyla silindi.";
            }
            catch (InvalidOperationException exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(MyAlbums));
        }

    }
}

