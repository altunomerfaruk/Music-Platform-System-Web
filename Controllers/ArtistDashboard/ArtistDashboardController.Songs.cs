using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicProject.Models.Concrete;
using MusicProject.ViewModels.ArtistDashboard;

namespace MusicProject.Controllers
{
    // Sanatcinin kendi sarkilari: listeleme, ekleme, duzenleme, silme.
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
        public IActionResult CreateSong()
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
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
                ModelState.AddModelError(nameof(model.Title), exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] =
                $"'{song.Title}' şarkısı başarıyla eklendi.";

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
                    .ToList()
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

            ValidateSongSelection(
                model.SelectedGenreIds,
                model.AlbumId,
                dashboard.Artist.Id,
                nameof(model.SelectedGenreIds),
                nameof(model.AlbumId),
                "Seçilen albüm size ait değil veya bulunamadı.");

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
                ModelState.AddModelError(nameof(model.Title), exception.Message);

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSong(int songId)
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            try
            {
                _songService.DeleteArtistSong(
                    songId,
                    dashboard.Artist.Id);

                TempData["SuccessMessage"] =
                    "Şarkı başarıyla silindi.";
            }
            catch (InvalidOperationException exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(MySongs));
        }

        /// <summary>
        /// Sarki formlarinda ortak dogrulama: en az bir tur + albumun sanatciya ait olmasi.
        /// </summary>
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

        private void FillCreateSongOptions(
            CreateSongViewModel model,
            int artistId)
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

        private void FillEditSongOptions(
            EditSongViewModel model,
            int artistId)
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
                    Selected =
                        model.SelectedGenreIds?.Contains(genre.Id) == true
                })
                .ToList();
        }
    }
}
