using Microsoft.AspNetCore.Mvc;
using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.ArtistDashboard;
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
                .GetAlbumsByArtistId(dashboard.Artist.ArtistId)
                .ToList();

            var model = new ArtistAlbumsViewModel
            {
                Albums = albums
                    .Select(ToArtistAlbumListItem)
                    .ToList(),
                TotalAlbumSongs = albums.Sum(album => album.Songs.Count),
                TotalAlbumStreams = albums
                    .SelectMany(album => album.Songs)
                    .DistinctBy(song => song.Id)
                    .Sum(song => song.SongStat?.TotalStreams ?? 0)
            };

            FillArtistLayoutData(model, dashboard);

            return View(model);
        }

        [HttpGet]
        public IActionResult AlbumDetails(int albumId)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            var album = _albumService.GetArtistAlbumDetails(albumId, dashboard.Artist.ArtistId);

            if (album == null)
            {
                TempData["ErrorMessage"] =
                    "Albüm bulunamadı veya bu albümü görüntüleme yetkiniz yok.";

                return RedirectToAction(nameof(MyAlbums));
            }

            var model = new ArtistAlbumDetailsViewModel
            {
                Album = new ArtistAlbumDetailsDto
                {
                    AlbumId = album.Id,
                    Name = album.Name,
                    Description = album.Description,
                    CoverImageUrl = album.CoverImageUrl,
                    ReleaseDate = album.ReleaseDate,
                    IsAdminHidden = album.IsAdminHidden,
                    AdminHiddenReason = album.AdminHiddenReason,
                    AdminHiddenAtUtc = album.AdminHiddenAtUtc,
                    Songs = album.Songs
                        .OrderBy(song => song.Title)
                        .Select(ToArtistSongListItem)
                        .ToList()
                },
                SongCount = album.Songs.Count,
                TotalStreams = album.Songs.Sum(song => song.SongStat?.TotalStreams ?? 0),
                TotalLikes = album.Songs.Sum(song => song.SongStat?.TotalLikes ?? 0)
            };

            FillArtistLayoutData(model, dashboard);

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
                ReleaseDate = DateTime.Today
            };

            FillArtistLayoutData(model, dashboard);

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

            if (model.PublicationStatus != PublicationStatus.Draft &&
                model.PublicationStatus != PublicationStatus.Scheduled)
            {
                ModelState.AddModelError(
                    nameof(model.PublicationStatus),
                    "Yeni bir albüm yalnızca taslak olarak kaydedilebilir veya cuma günü için planlanabilir.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = _artistAlbumWorkflowService.CreateAlbum(
                new CreateArtistAlbumRequest
                {
                    ArtistId = dashboard.Artist.ArtistId,
                    Name = model.Name,
                    Description = model.Description,
                    CoverImageUrl = model.CoverImageUrl,
                    ReleaseDate = model.ReleaseDate,
                    RequestedStatus = model.PublicationStatus,
                    ScheduledPublishAtLocal = model.ScheduledPublishAtLocal
                });

            if (!result.Succeeded)
            {
                return HandleAlbumWorkflowFailure(
                    result,
                    model,
                    nameof(model.Name),
                    nameof(model.ScheduledPublishAtLocal));
            }

            TempData["SuccessMessage"] = result.SuccessMessage;

            return RedirectToAction(nameof(MyAlbums));
        }

        [HttpGet]
        public IActionResult EditAlbum(int albumId)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            var album = _albumService.GetArtistAlbumDetails(albumId, dashboard.Artist.ArtistId);

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
                    : null,
                IsAdminHidden = album.IsAdminHidden,
                AdminHiddenReason = album.AdminHiddenReason,
                AdminHiddenAtUtc = album.AdminHiddenAtUtc
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
                dashboard.Artist.ArtistId);

            if (existingAlbum == null)
            {
                TempData["ErrorMessage"] =
                    "Albüm bulunamadı veya bu albümü düzenleme yetkiniz yok.";

                return RedirectToAction(nameof(MyAlbums));
            }

            model.IsAdminHidden = existingAlbum.IsAdminHidden;
            model.AdminHiddenReason = existingAlbum.AdminHiddenReason;
            model.AdminHiddenAtUtc = existingAlbum.AdminHiddenAtUtc;

            ValidateAlbumStatusTransition(
                model.PublicationStatus,
                existingAlbum.PublicationStatus,
                nameof(model.PublicationStatus));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = _artistAlbumWorkflowService.UpdateAlbum(
                new UpdateArtistAlbumRequest
                {
                    AlbumId = model.AlbumId,
                    ArtistId = dashboard.Artist.ArtistId,
                    Name = model.Name,
                    Description = model.Description,
                    CoverImageUrl = model.CoverImageUrl,
                    ReleaseDate = model.ReleaseDate,
                    RequestedStatus = model.PublicationStatus,
                    ScheduledPublishAtLocal = model.ScheduledPublishAtLocal
                });

            if (!result.Succeeded)
            {
                return HandleAlbumWorkflowFailure(
                    result,
                    model,
                    nameof(model.Name),
                    nameof(model.ScheduledPublishAtLocal));
            }

            TempData["SuccessMessage"] = result.SuccessMessage;

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

            var result = _artistAlbumWorkflowService.DeleteAlbum(
                albumId,
                dashboard.Artist.ArtistId);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = result.SuccessMessage;
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(MyAlbums));
        }

        private void ValidateAlbumStatusTransition(
            PublicationStatus requestedStatus,
            PublicationStatus currentStatus,
            string statusFieldName)
        {
            if (requestedStatus == PublicationStatus.Published &&
                currentStatus != PublicationStatus.Published)
            {
                ModelState.AddModelError(
                    statusFieldName,
                    "Albüm doğrudan yayınlanamaz. Albümü gelecek bir cuma günü için planlamalısın.");
            }

            if (currentStatus == PublicationStatus.Published &&
                requestedStatus != PublicationStatus.Published &&
                requestedStatus != PublicationStatus.Archived)
            {
                ModelState.AddModelError(
                    statusFieldName,
                    "Yayınlanmış bir albüm yalnızca yayında bırakılabilir veya arşivlenebilir.");
            }
        }

        private IActionResult HandleAlbumWorkflowFailure(
            ArtistAlbumWorkflowResult result,
            object model,
            string nameFieldName,
            string scheduledPublishFieldName)
        {
            var fieldName = result.ErrorField switch
            {
                ArtistAlbumWorkflowField.Name => nameFieldName,
                ArtistAlbumWorkflowField.ScheduledPublishAt => scheduledPublishFieldName,
                _ => null
            };

            if (fieldName == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                return RedirectToAction(nameof(MyAlbums));
            }

            ModelState.AddModelError(fieldName, result.ErrorMessage!);

            return View(model);
        }

        private static ArtistAlbumListItemDto ToArtistAlbumListItem(Album album)
        {
            return new ArtistAlbumListItemDto
            {
                AlbumId = album.Id,
                Name = album.Name,
                Description = album.Description,
                CoverImageUrl = album.CoverImageUrl,
                ReleaseDate = album.ReleaseDate,
                SongCount = album.Songs.Count,
                TotalStreams = album.Songs
                    .Sum(song => song.SongStat?.TotalStreams ?? 0),
                IsAdminHidden = album.IsAdminHidden,
                AdminHiddenReason = album.AdminHiddenReason,
                AdminHiddenAtUtc = album.AdminHiddenAtUtc
            };
        }
    }
}
