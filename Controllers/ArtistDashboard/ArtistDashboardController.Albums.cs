using Microsoft.AspNetCore.Mvc;
using MusicProject.Contracts.Requests;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
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
        public IActionResult AlbumDetails(int albumId)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            var album = _albumService.GetArtistAlbumDetails(albumId, dashboard.Artist.Id);

            if (album == null)
            {
                TempData["ErrorMessage"] =
                    "Albüm bulunamadı veya bu albümü görüntüleme yetkiniz yok.";

                return RedirectToAction(nameof(MyAlbums));
            }

            var model = new ArtistAlbumDetailsViewModel
            {
                Artist = dashboard.Artist,
                ArtistInitial = dashboard.ArtistInitial,
                TotalAlbums = dashboard.TotalAlbums,
                TotalSongs = dashboard.TotalSongs,
                Album = album,
                SongCount = album.Songs.Count,
                TotalStreams = album.Songs.Sum(song => song.SongStat?.TotalStreams ?? 0),
                TotalLikes = album.Songs.Sum(song => song.SongStat?.TotalLikes ?? 0)
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

            if (model.PublicationStatus == PublicationStatus.Archived)
            {
                ModelState.AddModelError(
                    nameof(model.PublicationStatus),
                    "Yeni bir albüm arşivlenmiş durumda oluşturulamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            DateTime? scheduledPublishAtUtc;

            try
            {
                scheduledPublishAtUtc = _publicationService.ValidateAndConvertToUtc(
                    model.PublicationStatus,
                    model.ScheduledPublishAtLocal);
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(
                    nameof(model.ScheduledPublishAtLocal),
                    exception.Message);

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
                PublicationStatus = model.PublicationStatus,
                ScheduledPublishAtUtc = scheduledPublishAtUtc,
                PublishedAtUtc = model.PublicationStatus == PublicationStatus.Published
                    ? DateTime.UtcNow
                    : null,
                ArtistId = dashboard.Artist.Id
            };

            try
            {
                _albumService.AddAlbum(album);

                if (album.PublicationStatus == PublicationStatus.Scheduled &&
                    album.ScheduledPublishAtUtc.HasValue)
                {
                    album.PublicationJobId =
                        _publicationJobScheduler.ScheduleAlbumPublication(
                            album.Id,
                            album.ScheduledPublishAtUtc.Value);

                    _albumService.UpdatePublication(album);
                }
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(nameof(model.Name), exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] = GetAlbumCreationMessage(album);

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
                ReleaseDate = album.ReleaseDate,
                PublicationStatus = album.PublicationStatus,
                ScheduledPublishAtLocal = album.ScheduledPublishAtUtc.HasValue
                    ? _publicationService.ConvertUtcToTurkeyTime(album.ScheduledPublishAtUtc.Value)
                    : null
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

            var existingAlbum = _albumService.GetArtistAlbumDetails(
                model.AlbumId,
                dashboard.Artist.Id);

            if (existingAlbum == null)
            {
                TempData["ErrorMessage"] =
                    "Albüm bulunamadı veya bu albümü düzenleme yetkiniz yok.";

                return RedirectToAction(nameof(MyAlbums));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            DateTime? scheduledPublishAtUtc;

            try
            {
                scheduledPublishAtUtc = _publicationService.ValidateAndConvertToUtc(
                    model.PublicationStatus,
                    model.ScheduledPublishAtLocal);
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(
                    nameof(model.ScheduledPublishAtLocal),
                    exception.Message);

                return View(model);
            }

            var publishedAtUtc = existingAlbum.PublishedAtUtc;

            if (model.PublicationStatus == PublicationStatus.Published &&
                !publishedAtUtc.HasValue)
            {
                publishedAtUtc = DateTime.UtcNow;
            }

            if (!string.IsNullOrWhiteSpace(existingAlbum.PublicationJobId))
            {
                _publicationJobScheduler.Cancel(existingAlbum.PublicationJobId);
            }

            string? newPublicationJobId = null;

            if (model.PublicationStatus == PublicationStatus.Scheduled &&
                scheduledPublishAtUtc.HasValue)
            {
                newPublicationJobId =
                    _publicationJobScheduler.ScheduleAlbumPublication(
                        model.AlbumId,
                        scheduledPublishAtUtc.Value);
            }

            var request = new UpdateAlbumRequest
            {
                AlbumId = model.AlbumId,
                ArtistId = dashboard.Artist.Id,
                Name = model.Name,
                Description = model.Description,
                CoverImageUrl = model.CoverImageUrl,
                ReleaseDate = model.ReleaseDate,
                PublicationStatus = model.PublicationStatus,
                ScheduledPublishAtUtc = scheduledPublishAtUtc,
                PublishedAtUtc = publishedAtUtc,
                PublicationJobId = newPublicationJobId
            };

            try
            {
                var updated = _albumService.UpdateArtistAlbum(request);

                if (!updated)
                {
                    if (!string.IsNullOrWhiteSpace(newPublicationJobId))
                    {
                        _publicationJobScheduler.Cancel(newPublicationJobId);
                    }

                    TempData["ErrorMessage"] =
                        "Albüm bulunamadı veya bu albümü düzenleme yetkiniz yok.";

                    return RedirectToAction(nameof(MyAlbums));
                }
            }
            catch (InvalidOperationException exception)
            {
                if (!string.IsNullOrWhiteSpace(newPublicationJobId))
                {
                    _publicationJobScheduler.Cancel(newPublicationJobId);
                }

                ModelState.AddModelError(nameof(model.Name), exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] = GetAlbumUpdateMessage(request);

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

            var album = _albumService.GetArtistAlbumDetails(albumId, dashboard.Artist.Id);

            if (album == null)
            {
                TempData["ErrorMessage"] =
                    "Albüm bulunamadı veya bu albümü silme yetkiniz yok.";

                return RedirectToAction(nameof(MyAlbums));
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

                _publicationJobScheduler.Cancel(album.PublicationJobId);

                TempData["SuccessMessage"] = "Albüm başarıyla silindi.";
            }
            catch (InvalidOperationException exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(MyAlbums));
        }

        private string GetAlbumCreationMessage(Album album)
        {
            return album.PublicationStatus switch
            {
                PublicationStatus.Draft =>
                    $"'{album.Name}' albümü taslak olarak kaydedildi.",

                PublicationStatus.Scheduled when album.ScheduledPublishAtUtc.HasValue =>
                    $"'{album.Name}' albümü " +
                    $"{_publicationService.ConvertUtcToTurkeyTime(album.ScheduledPublishAtUtc.Value):dd.MM.yyyy HH:mm} " +
                    "tarihine planlandı.",

                PublicationStatus.Published =>
                    $"'{album.Name}' albümü yayınlandı.",

                _ =>
                    $"'{album.Name}' albümü başarıyla oluşturuldu."
            };
        }

        private string GetAlbumUpdateMessage(UpdateAlbumRequest request)
        {
            return request.PublicationStatus switch
            {
                PublicationStatus.Draft =>
                    $"'{request.Name.Trim()}' albümü taslak olarak güncellendi.",

                PublicationStatus.Scheduled when request.ScheduledPublishAtUtc.HasValue =>
                    $"'{request.Name.Trim()}' albümü " +
                    $"{_publicationService.ConvertUtcToTurkeyTime(request.ScheduledPublishAtUtc.Value):dd.MM.yyyy HH:mm} " +
                    "tarihine planlandı.",

                PublicationStatus.Published =>
                    $"'{request.Name.Trim()}' albümü yayınlandı.",

                PublicationStatus.Archived =>
                    $"'{request.Name.Trim()}' albümü arşivlendi.",

                _ =>
                    $"'{request.Name.Trim()}' albümü başarıyla güncellendi."
            };
        }
    }
}