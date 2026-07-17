using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.ViewModels;
using MusicProject.Services.Interface;

namespace MusicProject.Controllers
{
    [Authorize] 
    public class ArtistController : Controller
    {
        private readonly IArtistService _artistService;

        // Dependency Injection ile Service'i alıyoruz
        public ArtistController(IArtistService artistService)
        {
            _artistService = artistService;
        }

        // --- 1. SANATÇILARI LİSTELEME SAYFASI ---
        public IActionResult Index()
        {
            var artists = _artistService.GetAllArtists();
            return View(artists);
        }

        // --- 2. İSTATİSTİK SAYFASI ---
        public IActionResult Statistics(int id)
        {
            var artist = _artistService.GetArtistById(id);

            if (artist == null)
            {
                return NotFound();
            }

            int totalSongs = _artistService.GetTotalSongCount(id);

            var model = new ArtistStatsViewModel
            {
                ArtistId = artist.Id,
                FullName = artist.Name, 
                TotalSongCount = totalSongs
            };

            return View(model);
        }
    }
}