using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.Concrete;
using MusicProject.ViewModels.UserDashboard;

namespace MusicProject.Controllers
{
    // Sarki listeleme / detay / begenme / dinleme aksiyonlari.
    public partial class UserDashboardController
    {
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
        public IActionResult AllSongs(
            string? search,
            int? artistId,
            int? albumId,
            int? genreId,
            string? sort,
            bool likedOnly = false)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            search = search?.Trim() ?? string.Empty;
            sort = string.IsNullOrWhiteSpace(sort) ? "name-asc" : sort;

            var allSongs = _songService
                .GetSongsSortedByAlphabet()
                .ToList();

            var likedSongIds = _likedSongService
                .GetActiveLikedSongIds(userId)
                .ToHashSet();

            var filteredSongs = ApplySongFilters(
                allSongs,
                search,
                artistId,
                albumId,
                genreId,
                likedOnly,
                likedSongIds);

            filteredSongs = ApplySongSort(filteredSongs, sort);

            var model = new AllSongsViewModel
            {
                Songs = filteredSongs.ToList(),
                LikedSongIds = likedSongIds,

                Search = search,
                ArtistId = artistId,
                AlbumId = albumId,
                GenreId = genreId,
                Sort = sort,
                LikedOnly = likedOnly,

                Artists = _artistService
                    .GetAllArtists()
                    .OrderBy(artist => artist.Name)
                    .ToList(),

                Albums = _albumService
                    .GetAllAlbums()
                    .OrderBy(album => album.Name)
                    .ToList(),

                Genres = _genreService
                    .GetAllGenres()
                    .OrderBy(genre => genre.Name)
                    .ToList(),

                TotalSongCount = allSongs.Count
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

            return RedirectBack(returnUrl);
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

        private static IEnumerable<Song> ApplySongFilters(
            IEnumerable<Song> songs,
            string search,
            int? artistId,
            int? albumId,
            int? genreId,
            bool likedOnly,
            HashSet<int> likedSongIds)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                songs = songs.Where(song =>
                    song.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (song.Album != null &&
                     song.Album.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (song.Album?.Artist != null &&
                     song.Album.Artist.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    song.SongArtists.Any(songArtist =>
                        songArtist.Artist.Name.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase
                        )));
            }

            if (artistId.HasValue)
            {
                songs = songs.Where(song =>
                    song.Album?.ArtistId == artistId.Value ||
                    song.SongArtists.Any(songArtist =>
                        songArtist.ArtistId == artistId.Value));
            }

            if (albumId.HasValue)
            {
                songs = songs.Where(song =>
                    song.AlbumId == albumId.Value);
            }

            if (genreId.HasValue)
            {
                songs = songs.Where(song =>
                    song.SongGenres.Any(songGenre =>
                        songGenre.GenreId == genreId.Value));
            }

            if (likedOnly)
            {
                songs = songs.Where(song =>
                    likedSongIds.Contains(song.Id));
            }

            return songs;
        }

        private static IEnumerable<Song> ApplySongSort(
            IEnumerable<Song> songs,
            string sort)
        {
            return sort switch
            {
                "name-desc" => songs
                    .OrderByDescending(song => song.Title),

                "streams-desc" => songs
                    .OrderByDescending(song => song.SongStat?.TotalStreams ?? 0)
                    .ThenBy(song => song.Title),

                "streams-asc" => songs
                    .OrderBy(song => song.SongStat?.TotalStreams ?? 0)
                    .ThenBy(song => song.Title),

                "newest" => songs
                    .OrderByDescending(song => song.CreatedAt)
                    .ThenBy(song => song.Title),

                "oldest" => songs
                    .OrderBy(song => song.CreatedAt)
                    .ThenBy(song => song.Title),

                _ => songs.OrderBy(song => song.Title)
            };
        }
    }
}
