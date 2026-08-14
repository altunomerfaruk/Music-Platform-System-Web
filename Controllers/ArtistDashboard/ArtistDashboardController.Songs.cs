using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IActionResult MySongs()
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            var songs = _songService
                .GetSongsByArtistId(dashboard.Artist.ArtistId)
                .ToList();

            var model = new ArtistSongsViewModel
            {
                Songs = songs
                    .Select(ToArtistSongListItem)
                    .ToList(),
                TotalStreams = songs.Sum(song => song.SongStat?.TotalStreams ?? 0),
                TotalLikes = songs.Sum(song => song.SongStat?.TotalLikes ?? 0)
            };

            FillArtistLayoutData(model, dashboard);

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
                    dashboard.Artist.ArtistId);

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
            FillCreateSongOptions(model, dashboard.Artist.ArtistId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSong(CreateSongViewModel model)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            model.SelectedGenreIds ??= new List<int>();

            FillArtistLayoutData(model, dashboard);
            FillCreateSongOptions(model, dashboard.Artist.ArtistId);

            ValidateSongSelection(
                model.SelectedGenreIds,
                model.AlbumId,
                dashboard.Artist.ArtistId,
                nameof(model.SelectedGenreIds),
                nameof(model.AlbumId),
                "Seçilen albüm bu sanatçı hesabına ait değil.");

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

            var result = await _artistSongWorkflowService.CreateSongAsync(
                new CreateArtistSongRequest
                {
                    ArtistId = dashboard.Artist.ArtistId,
                    Title = model.Title,
                    AlbumId = model.AlbumId,
                    LabelId = model.LabelId,
                    GenreIds = model.SelectedGenreIds,
                    RequestedStatus = model.PublicationStatus,
                    ScheduledPublishAtLocal = model.ScheduledPublishAtLocal,
                    AudioFile = model.AudioFile!
                });

            if (!result.Succeeded)
            {
                return HandleSongWorkflowFailure(
                    result,
                    model,
                    nameof(model.Title),
                    nameof(model.AudioFile),
                    nameof(model.ScheduledPublishAtLocal),
                    nameof(model.AlbumId));
            }

            TempData["SuccessMessage"] = result.SuccessMessage;

            if (result.AlbumId.HasValue)
            {
                return RedirectToAction(
                    nameof(AlbumDetails),
                    new { albumId = result.AlbumId.Value });
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
                dashboard.Artist.ArtistId);

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
                HasAudioFile = !string.IsNullOrWhiteSpace(song.AudioFileName),
                IsAdminHidden = song.IsAdminHidden,
                AdminHiddenReason = song.AdminHiddenReason,
                AdminHiddenAtUtc = song.AdminHiddenAtUtc,

                IsHiddenByAlbum = song.Album?.IsAdminHidden ?? false,
                AlbumAdminHiddenReason = song.Album?.AdminHiddenReason
            };

            FillArtistLayoutData(model, dashboard);
            FillEditSongOptions(model, dashboard.Artist.ArtistId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSong(EditSongViewModel model)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            model.SelectedGenreIds ??= new List<int>();

            FillArtistLayoutData(model, dashboard);
            FillEditSongOptions(model, dashboard.Artist.ArtistId);

            var existingSong = _songService.GetArtistSongForEdit(
                model.SongId,
                dashboard.Artist.ArtistId);

            if (existingSong == null)
            {
                TempData["ErrorMessage"] =
                    "Şarkı bulunamadı veya bu şarkıyı düzenleme yetkiniz yok.";

                return RedirectToAction(nameof(MySongs));
            }

            model.HasAudioFile =
                !string.IsNullOrWhiteSpace(existingSong.AudioFileName);

            model.IsAdminHidden = existingSong.IsAdminHidden;
            model.AdminHiddenReason = existingSong.AdminHiddenReason;
            model.AdminHiddenAtUtc = existingSong.AdminHiddenAtUtc;

            model.IsHiddenByAlbum =
                existingSong.Album?.IsAdminHidden ?? false;

            model.AlbumAdminHiddenReason =
                existingSong.Album?.AdminHiddenReason;

            ValidateSongSelection(
                model.SelectedGenreIds,
                model.AlbumId,
                dashboard.Artist.ArtistId,
                nameof(model.SelectedGenreIds),
                nameof(model.AlbumId),
                "Seçilen albüm bu sanatçı hesabına ait değil.");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _artistSongWorkflowService.UpdateSongAsync(
                new UpdateArtistSongRequest
                {
                    SongId = model.SongId,
                    ArtistId = dashboard.Artist.ArtistId,
                    Title = model.Title,
                    AlbumId = model.AlbumId,
                    LabelId = model.LabelId,
                    GenreIds = model.SelectedGenreIds,
                    RequestedStatus = model.PublicationStatus,
                    ScheduledPublishAtLocal = model.ScheduledPublishAtLocal,
                    AudioFile = model.AudioFile
                });

            if (!result.Succeeded)
            {
                return HandleSongWorkflowFailure(
                    result,
                    model,
                    nameof(model.Title),
                    nameof(model.AudioFile),
                    nameof(model.ScheduledPublishAtLocal),
                    nameof(model.AlbumId));
            }

            TempData["SuccessMessage"] = result.SuccessMessage;

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

            var result = _artistSongWorkflowService.DeleteSong(
                songId,
                dashboard.Artist.ArtistId);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = result.SuccessMessage;
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(MySongs));
        }

        private static ArtistSongListItemDto ToArtistSongListItem(Song song)
        {
            return new ArtistSongListItemDto
            {
                SongId = song.Id,
                Title = song.Title,
                AlbumId = song.AlbumId,
                AlbumName = song.Album?.Name ?? string.Empty,
                TotalStreams = song.SongStat?.TotalStreams ?? 0,
                TotalLikes = song.SongStat?.TotalLikes ?? 0,
                PopularityScore = song.SongStat?.PopularityScore ?? 0,
                PublicationStatus = song.PublicationStatus,
                CreatedAt = song.CreatedAt,
                IsAdminHidden = song.IsAdminHidden,
                AdminHiddenReason = song.AdminHiddenReason,
                IsHiddenByAlbum = song.Album?.IsAdminHidden ?? false,
                AlbumAdminHiddenReason = song.Album?.AdminHiddenReason,
                GenreNames = song.SongGenres
                    .Where(songGenre => songGenre.Genre != null)
                    .Select(songGenre => songGenre.Genre.Name)
                    .Distinct()
                    .OrderBy(genreName => genreName)
                    .ToList()
            };
        }

        private IActionResult HandleSongWorkflowFailure(
            ArtistSongWorkflowResult result,
            object model,
            string titleFieldName,
            string audioFileFieldName,
            string scheduledPublishFieldName,
            string albumFieldName)
        {
            var fieldName = result.ErrorField switch
            {
                ArtistSongWorkflowField.Title => titleFieldName,
                ArtistSongWorkflowField.AudioFile => audioFileFieldName,
                ArtistSongWorkflowField.ScheduledPublishAt => scheduledPublishFieldName,
                ArtistSongWorkflowField.AlbumId => albumFieldName,
                _ => null
            };

            if (fieldName == null)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                return RedirectToAction(nameof(MySongs));
            }

            ModelState.AddModelError(fieldName, result.ErrorMessage!);

            return View(model);
        }

        private void ValidateSongSelection(
            List<int> selectedGenreIds,
            int? albumId,
            int artistId,
            string genreFieldName,
            string albumFieldName,
            string albumErrorMessage)
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
                    ModelState.AddModelError(albumFieldName, albumErrorMessage);
                }
            }
        }

        private void FillCreateSongOptions(CreateSongViewModel model, int artistId)
        {
            model.AlbumOptions = BuildAlbumOptions(artistId, model.AlbumId);
            model.GenreOptions = BuildGenreOptions(model.SelectedGenreIds);
        }

        private void FillEditSongOptions(EditSongViewModel model, int artistId)
        {
            model.AlbumOptions = BuildAlbumOptions(artistId, model.AlbumId);
            model.GenreOptions = BuildGenreOptions(model.SelectedGenreIds);
        }

        private List<SelectListItem> BuildAlbumOptions(int artistId, int? selectedAlbumId)
        {
            return _albumService
                .GetAlbumsByArtistId(artistId)
                .OrderBy(album => album.Name)
                .Select(album => new SelectListItem
                {
                    Value = album.Id.ToString(),
                    Text = album.Name,
                    Selected = selectedAlbumId == album.Id
                })
                .ToList();
        }

        private List<SelectListItem> BuildGenreOptions(ICollection<int> selectedGenreIds)
        {
            return _genreService
                .GetAllGenres()
                .OrderBy(genre => genre.Name)
                .Select(genre => new SelectListItem
                {
                    Value = genre.Id.ToString(),
                    Text = genre.Name,
                    Selected = selectedGenreIds.Contains(genre.Id)
                })
                .ToList();
        }
    }
}
