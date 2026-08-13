using Microsoft.AspNetCore.Mvc;
using MusicProject.Contracts.Responses.UserDashboard;
using MusicProject.Models.Concrete;
using MusicProject.ViewModels.UserDashboard;

namespace MusicProject.Controllers
{
    public partial class UserDashboardController
    {
        private const int SuggestionLimit = 4;

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
                    .SearchSongsByText(query, maxResults: null)
                    .Select(ToSongListItem)
                    .ToList();

                model.Artists = _artistService
                    .SearchArtistsByText(query, maxResults: null)
                    .Select(ToArtistListItem)
                    .ToList();

                model.Albums = _albumService
                    .SearchAlbumsByText(query, maxResults: null)
                    .Select(ToAlbumListItem)
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
                .SearchSongsByText(query, SuggestionLimit)
                .Select(song => new
                {
                    id = song.Id,
                    name = song.Title,
                    subtitle = song.Album?.Name ?? "Single",
                    type = "song"
                })
                .ToList();

            var artists = _artistService
                .SearchArtistsByText(query, SuggestionLimit)
                .Select(artist => new
                {
                    id = artist.Id,
                    name = artist.Name,
                    subtitle = "Sanatçı",
                    type = "artist"
                })
                .ToList();

            var albums = _albumService
                .SearchAlbumsByText(query, SuggestionLimit)
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

        private static AlbumListItemDto ToAlbumListItem(Album album)
        {
            return new AlbumListItemDto
            {
                AlbumId = album.Id,
                Name = album.Name,
                ArtistName = album.Artist.Name
            };
        }
    }
}
