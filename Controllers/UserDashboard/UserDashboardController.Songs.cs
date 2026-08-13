using Microsoft.AspNetCore.Mvc;
using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.UserDashboard;
using MusicProject.Models.Concrete;
using MusicProject.ViewModels.UserDashboard;

namespace MusicProject.Controllers
{
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

            var likedSongIds = _likedSongService
                .GetActiveLikedSongIds(userId)
                .ToHashSet();

            var songs = _songService.SearchSongs(new SongSearchRequest
            {
                Search = search,
                ArtistId = artistId,
                AlbumId = albumId,
                GenreId = genreId,
                Sort = sort,
                LikedOnly = likedOnly,
                UserId = userId
            });

            var model = new AllSongsViewModel
            {
                Songs = songs
                    .Select(ToSongListItem)
                    .ToList(),

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
                    .Select(artist => new FilterOptionDto
                    {
                        Id = artist.Id,
                        Name = artist.Name
                    })
                    .ToList(),

                Albums = _albumService
                    .GetAllAlbums()
                    .OrderBy(album => album.Name)
                    .Select(album => new FilterOptionDto
                    {
                        Id = album.Id,
                        Name = album.Name
                    })
                    .ToList(),

                Genres = _genreService
                    .GetAllGenres()
                    .OrderBy(genre => genre.Name)
                    .Select(genre => new FilterOptionDto
                    {
                        Id = genre.Id,
                        Name = genre.Name
                    })
                    .ToList(),

                TotalSongCount = _songService.GetVisibleSongCount()
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

            if (_songService.GetSongDetails(songId) == null)
            {
                return NotFound("Şarkı bulunamadı.");
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

            if (songId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Geçersiz şarkı bilgisi."
                });
            }

            var song = _songService.GetSongForListening(songId);

            if (song == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Şarkı bulunamadı veya şu anda dinlenemiyor."
                });
            }

            if (string.IsNullOrWhiteSpace(song.AudioFileName))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Bu şarkının henüz bir MP3 dosyası bulunmuyor."
                });
            }

            var isAdded = _listeningHistoryService.AddListening(userId, songId);

            if (!isAdded)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dinleme kaydı oluşturulamadı."
                });
            }

            return Json(new
            {
                success = true,
                message = "Şarkı çalınıyor.",
                streamUrl = Url.Action(nameof(StreamSong), "UserDashboard", new { songId })
            });
        }

        [HttpGet]
        public IActionResult StreamSong(int songId)
        {
            if (songId <= 0)
                return BadRequest();

            var song = _songService.GetSongForListening(songId);

            if (song == null || string.IsNullOrWhiteSpace(song.AudioFileName))
                return NotFound();

            var audioStream = _audioStorageService.OpenRead(song.AudioFileName);

            if (audioStream == null)
                return NotFound();

            return File(
                audioStream,
                "audio/mpeg",
                enableRangeProcessing: true);
        }

        private static SongListItemDto ToSongListItem(Song song)
        {
            var artist = song.Album?.Artist ??
                         song.SongArtists
                             .Select(songArtist => songArtist.Artist)
                             .FirstOrDefault();

            return new SongListItemDto
            {
                SongId = song.Id,
                Title = song.Title,
                AlbumId = song.AlbumId,
                AlbumName = song.Album?.Name ?? string.Empty,
                ArtistId = artist?.Id,
                ArtistName = artist?.Name ?? string.Empty,
                TotalStreams = song.SongStat?.TotalStreams ?? 0
            };
        }
    }
}
