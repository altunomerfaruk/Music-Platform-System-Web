using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.ViewModels;
using MusicProject.Services.Interface;
using System.Security.Claims;

namespace MusicProject.Controllers
{
    [Authorize(Roles = "Artist")]
    public class ArtistDashboardController : Controller
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;

        public ArtistDashboardController(
            IArtistService artistService,
            IAlbumService albumService)
        {
            _artistService = artistService;
            _albumService = albumService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = _artistService.GetArtistDashboard(userId);

            if (model == null)
            {
                return View("ArtistProfileNotFound");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult MyAlbums()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View("ArtistProfileNotFound");
            }

            var albums = _albumService
                .GetAlbumsByArtistId(dashboard.Artist.Id)
                .ToList();

            var model = new ArtistAlbumsViewModel
            {
                Artist = dashboard.Artist,
                ArtistInitial = dashboard.ArtistInitial,
                TotalAlbums = dashboard.TotalAlbums,
                TotalSongs = dashboard.TotalSongs,
                Albums = albums,

                TotalAlbumSongs = albums
                    .Sum(album => album.Songs.Count),

                TotalAlbumStreams = albums
                    .SelectMany(album => album.Songs)
                    .DistinctBy(song => song.Id)
                    .Sum(song =>
                        song.SongStat?.TotalStreams ?? 0)
            };

            return View(model);
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            return int.TryParse(userIdValue, out userId);
        }
    }
}