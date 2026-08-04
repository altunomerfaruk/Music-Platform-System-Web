using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.Concrete;
using MusicProject.ViewModels.UserDashboard;

namespace MusicProject.Controllers
{
    // Sanatci listeleme / detay / takip aksiyonlari.
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

            var allArtists = _artistService
                .GetAllArtists()
                .ToList();

            var followedArtistIds = _followedArtistService
                .GetActiveFollowedArtistIds(userId)
                .ToHashSet();

            var countries = allArtists
                .Select(artist => artist.CountryEntity?.Name)
                .Where(countryName => !string.IsNullOrWhiteSpace(countryName))
                .Select(countryName => countryName!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(countryName => countryName)
                .ToList();

            var filteredArtists = ApplyArtistFilters(
                allArtists,
                search,
                country,
                followedOnly,
                followedArtistIds);

            filteredArtists = ApplyArtistSort(filteredArtists, sort);

            var model = new AllArtistsViewModel
            {
                Artists = filteredArtists.ToList(),
                FollowedArtistIds = followedArtistIds,
                Search = search,
                Country = country,
                Sort = sort,
                FollowedOnly = followedOnly,
                Countries = countries,
                TotalArtistCount = allArtists.Count
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

        private static IEnumerable<Artist> ApplyArtistFilters(
            IEnumerable<Artist> artists,
            string search,
            string country,
            bool followedOnly,
            HashSet<int> followedArtistIds)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                artists = artists.Where(artist =>
                    artist.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                artists = artists.Where(artist =>
                    artist.CountryEntity != null &&
                    artist.CountryEntity.Name.Equals(
                        country,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (followedOnly)
            {
                artists = artists.Where(artist =>
                    followedArtistIds.Contains(artist.Id));
            }

            return artists;
        }

        private static IEnumerable<Artist> ApplyArtistSort(
            IEnumerable<Artist> artists,
            string sort)
        {
            return sort switch
            {
                "name-desc" => artists.OrderByDescending(artist => artist.Name),

                "year-newest" => artists
                    .OrderByDescending(artist => artist.DebutYear.HasValue)
                    .ThenByDescending(artist => artist.DebutYear)
                    .ThenBy(artist => artist.Name),

                "year-oldest" => artists
                    .OrderByDescending(artist => artist.DebutYear.HasValue)
                    .ThenBy(artist => artist.DebutYear)
                    .ThenBy(artist => artist.Name),

                _ => artists.OrderBy(artist => artist.Name)
            };
        }
    }
}
