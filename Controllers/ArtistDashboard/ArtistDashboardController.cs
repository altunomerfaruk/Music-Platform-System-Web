using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Controllers.Base;
using MusicProject.Services.Interface;
using MusicProject.ViewModels.ArtistDashboard;

namespace MusicProject.Controllers
{
    [Authorize(Roles = "Artist")]
    public partial class ArtistDashboardController : DashboardControllerBase
    {
        private const string ProfileNotFoundView = "ArtistProfileNotFound";

        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly ISongService _songService;
        private readonly IGenreService _genreService;
        private readonly ICountryService _countryService;
        private readonly IPublicationService _publicationService;

        public ArtistDashboardController(IArtistService artistService, IAlbumService albumService, ISongService songService,
            IGenreService genreService, ICountryService countryService, IPublicationService publicationService)
        {
            _artistService = artistService;
            _albumService = albumService;
            _songService = songService;
            _genreService = genreService;
            _countryService = countryService;
            _publicationService = publicationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var dashboard = _artistService.GetArtistDashboard(userId);

            if (dashboard == null)
            {
                return View(ProfileNotFoundView);
            }

            return View(dashboard);
        }

        private bool TryGetDashboard(out ArtistDashboardViewModel dashboard, out int userId, out IActionResult? errorResult)
        {
            dashboard = null!;
            errorResult = null;

            if (!TryGetCurrentUserId(out userId))
            {
                errorResult = RedirectToLogin();
                return false;
            }

            var found = _artistService.GetArtistDashboard(userId);

            if (found == null)
            {
                errorResult = View(ProfileNotFoundView);
                return false;
            }

            dashboard = found;
            return true;
        }

        private static void FillArtistLayoutData(ArtistLayoutViewModel model, ArtistDashboardViewModel dashboard)
        {
            model.Artist = dashboard.Artist;
            model.ArtistInitial = dashboard.ArtistInitial;
            model.TotalAlbums = dashboard.TotalAlbums;
            model.TotalSongs = dashboard.TotalSongs;
        }
    }
}