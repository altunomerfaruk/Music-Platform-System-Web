using Microsoft.AspNetCore.Mvc;
using MusicProject.ViewModels.UserDashboard;

namespace MusicProject.Controllers
{
    // Arama sayfasi ve canli arama onerileri (JSON).
    public partial class UserDashboardController
    {
        [HttpGet]
        public IActionResult Search(string? query)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            query = query?.Trim() ?? string.Empty;

            var model = new SearchResultsViewModel
            {
                Query = query,

                LikedSongIds = _likedSongService
                    .GetActiveLikedSongIds(userId)
                    .ToHashSet(),

                FollowedArtistIds = _followedArtistService
                    .GetActiveFollowedArtistIds(userId)
                    .ToHashSet()
            };

            if (!string.IsNullOrWhiteSpace(query))
            {
                model.Songs = _songService
                    .GetSongsSortedByAlphabet()
                    .Where(song =>
                        song.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (song.Album != null &&
                         song.Album.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                model.Artists = _artistService
                    .GetAllArtists()
                    .Where(artist =>
                        artist.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (artist.CountryEntity != null &&
                         artist.CountryEntity.Name.Contains(
                             query,
                             StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(artist => artist.Name)
                    .ToList();

                model.Albums = _albumService
                    .GetAllAlbums()
                    .Where(album =>
                        album.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        album.Artist.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(album => album.Name)
                    .ToList();
            }

            FillLayoutData(model, userId);

            return View("SearchResults", model);
        }

        [HttpGet]
        public IActionResult SearchSuggestions(string? query)
        {
            query = query?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new
                {
                    songs = Array.Empty<object>(),
                    artists = Array.Empty<object>(),
                    albums = Array.Empty<object>()
                });
            }

            var songs = _songService
                .GetSongsSortedByAlphabet()
                .Where(song =>
                    song.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (song.Album != null &&
                     song.Album.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .Take(4)
                .Select(song => new
                {
                    id = song.Id,
                    name = song.Title,
                    subtitle = song.Album?.Name ?? "Single",
                    type = "song"
                })
                .ToList();

            var artists = _artistService
                .GetAllArtists()
                .Where(artist =>
                    artist.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(artist => artist.Name)
                .Take(4)
                .Select(artist => new
                {
                    id = artist.Id,
                    name = artist.Name,
                    subtitle = "Sanatçı",
                    type = "artist"
                })
                .ToList();

            var albums = _albumService
                .GetAllAlbums()
                .Where(album =>
                    album.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    album.Artist.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(album => album.Name)
                .Take(4)
                .Select(album => new
                {
                    id = album.Id,
                    name = album.Name,
                    subtitle = album.Artist.Name,
                    type = "album"
                })
                .ToList();

            return Json(new
            {
                songs,
                artists,
                albums
            });
        }
    }
}
