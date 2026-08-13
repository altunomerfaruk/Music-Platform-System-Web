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
        public IActionResult ArtistDetails(int artistId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            if (artistId <= 0)
            {
                return BadRequest("Geçersiz sanatçı bilgisi.");
            }

            var artist = _artistService.GetArtistDetails(artistId);

            if (artist == null)
            {
                return NotFound("Sanatçı bulunamadı.");
            }

            var followedArtistIds = _followedArtistService
                .GetActiveFollowedArtistIds(userId)
                .ToHashSet();

            var model = new ArtistDetailsViewModel
            {
                Artist = artist,
                IsFollowed = followedArtistIds.Contains(artistId),

                LikedSongIds = _likedSongService
                    .GetActiveLikedSongIds(userId)
                    .ToHashSet()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpGet]
        public IActionResult FollowedArtists()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var model = new FollowedArtistsViewModel
            {
                Artists = _followedArtistService
                    .GetFollowedArtistsByUser(userId)
                    .ToList()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpGet]
        public IActionResult AllArtists(
            string? search,
            string? country,
            string? sort,
            bool followedOnly = false)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            search = search?.Trim() ?? string.Empty;
            country = country?.Trim() ?? string.Empty;
            sort = string.IsNullOrWhiteSpace(sort) ? "name-asc" : sort;

            var followedArtistIds = _followedArtistService
                .GetActiveFollowedArtistIds(userId)
                .ToHashSet();

            var artists = _artistService.SearchArtists(new ArtistSearchRequest
            {
                Search = search,
                Country = country,
                Sort = sort,
                FollowedOnly = followedOnly,
                UserId = userId
            });

            var model = new AllArtistsViewModel
            {
                Artists = artists
                    .Select(ToArtistListItem)
                    .ToList(),

                FollowedArtistIds = followedArtistIds,
                Search = search,
                Country = country,
                Sort = sort,
                FollowedOnly = followedOnly,
                Countries = _artistService.GetUsedCountryNames().ToList(),
                TotalArtistCount = _artistService.GetArtistCount()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFollow(int artistId, string? returnUrl)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            if (artistId <= 0)
            {
                return BadRequest("Geçersiz sanatçı bilgisi.");
            }

            _followedArtistService.ToggleFollow(userId, artistId);

            return RedirectBack(returnUrl);
        }

        private static ArtistListItemDto ToArtistListItem(Artist artist)
        {
            return new ArtistListItemDto
            {
                ArtistId = artist.Id,
                Name = artist.Name,
                Country = artist.CountryEntity?.Name ?? string.Empty,
                DebutYear = artist.DebutYear
            };
        }
    }
}
