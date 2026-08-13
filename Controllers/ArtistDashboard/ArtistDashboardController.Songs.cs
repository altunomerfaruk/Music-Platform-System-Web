using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.ViewModels.ArtistDashboard;

namespace MusicProject.Controllers
{
    public partial class ArtistDashboardController
    {
        [HttpGet]
        public IActionResult MySongs()
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            var songs = _songService
                .GetSongsByArtistId(dashboard.Artist.Id)
                .ToList();

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
        public IActionResult CreateSong(int? albumId)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            if (albumId.HasValue)
            {
                var selectedAlbum = _albumService.GetArtistAlbumDetails(
                    albumId.Value,
                    dashboard.Artist.Id);

                if (selectedAlbum == null)
                {
                    TempData["ErrorMessage"] =
                        "Albüm bulunamadı veya bu albüme şarkı ekleme yetkiniz yok.";

                    return RedirectToAction(nameof(MyAlbums));
                }
            }

            var model = new CreateSongViewModel
            {
                AlbumId = albumId
            };

            FillArtistLayoutData(model, dashboard);
            FillCreateSongOptions(model, dashboard.Artist.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSong(CreateSongViewModel model)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            model.SelectedGenreIds ??= new List<int>();

            FillArtistLayoutData(model, dashboard);
            FillCreateSongOptions(model, dashboard.Artist.Id);

            ValidateSongSelection(
                model.SelectedGenreIds,
                model.AlbumId,
                dashboard.Artist.Id,
                nameof(model.SelectedGenreIds),
                nameof(model.AlbumId),
                "Seçilen albüm bu sanatçı hesabına ait değil.");

            Album? selectedAlbum = null;

            if (model.AlbumId.HasValue)
            {
                selectedAlbum = _albumService.GetArtistAlbumDetails(
                    model.AlbumId.Value,
                    dashboard.Artist.Id);
            }

            if (model.AlbumId == null &&
                model.PublicationStatus == PublicationStatus.Archived)
            {
                ModelState.AddModelError(
                    nameof(model.PublicationStatus),
                    "Yeni bir şarkı arşivlenmiş durumda oluşturulamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            DateTime? scheduledPublishAtUtc = null;
            DateTime? publishedAtUtc = null;
            var publicationStatus = PublicationStatus.Draft;

            if (selectedAlbum != null)
            {
                switch (selectedAlbum.PublicationStatus)
                {
                    case PublicationStatus.Published:
                        publicationStatus = PublicationStatus.Published;
                        publishedAtUtc = DateTime.UtcNow;
                        break;

                    case PublicationStatus.Draft:
                    case PublicationStatus.Scheduled:
                    case PublicationStatus.Archived:
                        publicationStatus = PublicationStatus.Draft;
                        break;
                }
            }
            else
            {
                publicationStatus = model.PublicationStatus;

                try
                {
                    scheduledPublishAtUtc =
                        _publicationService.ValidateAndConvertToUtc(
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

                if (publicationStatus == PublicationStatus.Published)
                {
                    publishedAtUtc = DateTime.UtcNow;
                }
            }

            var song = new Song
            {
                Title = model.Title.Trim(),
                AlbumId = model.AlbumId,
                LabelId = model.LabelId,
                PublicationStatus = publicationStatus,
                ScheduledPublishAtUtc = scheduledPublishAtUtc,
                PublishedAtUtc = publishedAtUtc
            };

            try
            {
                _songService.AddSongWithRelations(
                    song,
                    dashboard.Artist.Id,
                    model.SelectedGenreIds);

                if (song.AlbumId == null &&
                    song.PublicationStatus == PublicationStatus.Scheduled &&
                    song.ScheduledPublishAtUtc.HasValue)
                {
                    song.PublicationJobId =
                        _publicationJobScheduler.ScheduleSongPublication(
                            song.Id,
                            song.ScheduledPublishAtUtc.Value);

                    _songService.UpdatePublication(song);
                }
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] = GetSongCreationMessage(song);

            if (song.AlbumId.HasValue)
            {
                return RedirectToAction(
                    nameof(AlbumDetails),
                    new { albumId = song.AlbumId.Value });
            }

            return RedirectToAction(nameof(MySongs));
        }

        [HttpGet]
        public IActionResult EditSong(int songId)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            var song = _songService.GetArtistSongForEdit(
                songId,
                dashboard.Artist.Id);

            if (song == null)
            {
                TempData["ErrorMessage"] =
                    "Şarkı bulunamadı veya bu şarkıyı düzenleme yetkiniz yok.";

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
                                        .ToList(),
                PublicationStatus = song.PublicationStatus,
                ScheduledPublishAtLocal = song.ScheduledPublishAtUtc.HasValue
                ? _publicationService.ConvertUtcToTurkeyTime(song.ScheduledPublishAtUtc.Value)
                : null,

                IsAdminHidden = song.IsAdminHidden,
                AdminHiddenReason = song.AdminHiddenReason,
                AdminHiddenAtUtc = song.AdminHiddenAtUtc,

                IsHiddenByAlbum = song.Album?.IsAdminHidden ?? false,
                AlbumAdminHiddenReason = song.Album?.AdminHiddenReason
            };

            FillArtistLayoutData(model, dashboard);
            FillEditSongOptions(model, dashboard.Artist.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSong(EditSongViewModel model)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            model.SelectedGenreIds ??= new List<int>();

            FillArtistLayoutData(model, dashboard);
            FillEditSongOptions(model, dashboard.Artist.Id);

            var existingSong = _songService.GetArtistSongForEdit(
                model.SongId,
                dashboard.Artist.Id);


            if (existingSong == null)
            {
                TempData["ErrorMessage"] =
                    "Şarkı bulunamadı veya bu şarkıyı düzenleme yetkiniz yok.";

                return RedirectToAction(nameof(MySongs));
            }


            model.IsAdminHidden = existingSong.IsAdminHidden;
            model.AdminHiddenReason = existingSong.AdminHiddenReason;
            model.AdminHiddenAtUtc = existingSong.AdminHiddenAtUtc;
            model.IsHiddenByAlbum = existingSong.Album?.IsAdminHidden ?? false;
            model.AlbumAdminHiddenReason = existingSong.Album?.AdminHiddenReason;

            var oldPublicationJobId = existingSong.PublicationJobId;

            ValidateSongSelection(
                model.SelectedGenreIds,
                model.AlbumId,
                dashboard.Artist.Id,
                nameof(model.SelectedGenreIds),
                nameof(model.AlbumId),
                "Seçilen albüm bu sanatçı hesabına ait değil.");

            Album? selectedAlbum = null;

            if (model.AlbumId.HasValue)
            {
                selectedAlbum = _albumService.GetArtistAlbumDetails(
                    model.AlbumId.Value,
                    dashboard.Artist.Id);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            DateTime? scheduledPublishAtUtc = null;
            DateTime? publishedAtUtc = existingSong.PublishedAtUtc;
            var publicationStatus = PublicationStatus.Draft;
            string? newPublicationJobId = null;

            if (selectedAlbum != null)
            {
                publicationStatus = selectedAlbum.PublicationStatus switch
                {
                    PublicationStatus.Published => PublicationStatus.Published,
                    _ => PublicationStatus.Draft
                };

                if (publicationStatus == PublicationStatus.Published &&
                    !publishedAtUtc.HasValue)
                {
                    publishedAtUtc = DateTime.UtcNow;
                }
            }
            else
            {
                publicationStatus = model.PublicationStatus;

                try
                {
                    scheduledPublishAtUtc =
                        _publicationService.ValidateAndConvertToUtc(
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

                if (publicationStatus == PublicationStatus.Published &&
                    !publishedAtUtc.HasValue)
                {
                    publishedAtUtc = DateTime.UtcNow;
                }

                if (publicationStatus == PublicationStatus.Scheduled &&
                    scheduledPublishAtUtc.HasValue)
                {
                    newPublicationJobId =
                        _publicationJobScheduler.ScheduleSongPublication(
                            model.SongId,
                            scheduledPublishAtUtc.Value);
                }
            }

            var song = new Song
            {
                Id = model.SongId,
                Title = model.Title,
                AlbumId = model.AlbumId,
                LabelId = model.LabelId,
                PublicationStatus = publicationStatus,
                ScheduledPublishAtUtc = scheduledPublishAtUtc,
                PublishedAtUtc = publishedAtUtc,
                PublicationJobId = newPublicationJobId
            };

            try
            {
                _songService.UpdateArtistSong(
                    song,
                    dashboard.Artist.Id,
                    model.SelectedGenreIds);

                if (!string.IsNullOrWhiteSpace(oldPublicationJobId) &&
                    oldPublicationJobId != newPublicationJobId)
                {
                    _publicationJobScheduler.Cancel(oldPublicationJobId);
                }
            }
            catch (InvalidOperationException exception)
            {
                if (!string.IsNullOrWhiteSpace(newPublicationJobId))
                {
                    _publicationJobScheduler.Cancel(newPublicationJobId);
                }

                ModelState.AddModelError(
                    nameof(model.Title),
                    exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] = GetSongUpdateMessage(
                model.Title,
                publicationStatus,
                scheduledPublishAtUtc);

            return RedirectToAction(nameof(MySongs));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSong(int songId)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            var song = _songService.GetArtistSongForEdit(
                songId,
                dashboard.Artist.Id);

            if (song == null)
            {
                TempData["ErrorMessage"] =
                    "Şarkı bulunamadı veya bu şarkıyı silme yetkiniz yok.";

                return RedirectToAction(nameof(MySongs));
            }

            var publicationJobId = song.PublicationJobId;

            try
            {
                _songService.DeleteArtistSong(
                    songId,
                    dashboard.Artist.Id);

                _publicationJobScheduler.Cancel(publicationJobId);

                TempData["SuccessMessage"] =
                    "Şarkı başarıyla silindi.";
            }
            catch (InvalidOperationException exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(MySongs));
        }

        private void ValidateSongSelection(List<int> selectedGenreIds, int? albumId, int artistId, string genreFieldName, string albumFieldName, string albumErrorMessage)
        {
            if (selectedGenreIds.Count == 0)
            {
                ModelState.AddModelError(
                    genreFieldName,
                    "En az bir müzik türü seçmelisiniz.");
            }

            if (albumId.HasValue)
            {
                var selectedAlbum = _albumService.GetArtistAlbumDetails(
                    albumId.Value,
                    artistId);

                if (selectedAlbum == null)
                {
                    ModelState.AddModelError(
                        albumFieldName,
                        albumErrorMessage);
                }
            }
        }

        private void FillCreateSongOptions(CreateSongViewModel model, int artistId)
        {
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

        private void FillEditSongOptions(EditSongViewModel model, int artistId)
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
                    Selected = model.SelectedGenreIds.Contains(genre.Id)
                })
                .ToList();
        }

        private string GetSongCreationMessage(Song song)
        {
            return song.PublicationStatus switch
            {
                PublicationStatus.Draft =>
                    $"'{song.Title}' şarkısı taslak olarak kaydedildi.",

                PublicationStatus.Scheduled when song.ScheduledPublishAtUtc.HasValue =>
                    $"'{song.Title}' şarkısı " +
                    $"{_publicationService.ConvertUtcToTurkeyTime(song.ScheduledPublishAtUtc.Value):dd.MM.yyyy HH:mm} " +
                    "tarihine planlandı.",

                PublicationStatus.Published =>
                    $"'{song.Title}' şarkısı yayınlandı.",

                _ =>
                    $"'{song.Title}' şarkısı başarıyla oluşturuldu."
            };
        }

        private string GetSongUpdateMessage(string title, PublicationStatus publicationStatus, DateTime? scheduledPublishAtUtc)
        {
            return publicationStatus switch
            {
                PublicationStatus.Draft =>
                    $"'{title.Trim()}' şarkısı taslak olarak güncellendi.",

                PublicationStatus.Scheduled when scheduledPublishAtUtc.HasValue =>
                    $"'{title.Trim()}' şarkısı " +
                    $"{_publicationService.ConvertUtcToTurkeyTime(scheduledPublishAtUtc.Value):dd.MM.yyyy HH:mm} " +
                    "tarihine planlandı.",

                PublicationStatus.Published =>
                    $"'{title.Trim()}' şarkısı yayınlandı.",

                PublicationStatus.Archived =>
                    $"'{title.Trim()}' şarkısı arşivlendi.",

                _ =>
                    $"'{title.Trim()}' şarkısı başarıyla güncellendi."
            };
        }
    }
}