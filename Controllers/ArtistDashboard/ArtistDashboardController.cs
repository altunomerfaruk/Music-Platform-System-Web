using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Contracts.Responses.ArtistDashboard;
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

        private readonly IArtistSongWorkflowService _artistSongWorkflowService;
        private readonly IArtistAlbumWorkflowService _artistAlbumWorkflowService;

        public ArtistDashboardController(
            IArtistService artistService,
            IAlbumService albumService,
            ISongService songService,
            IGenreService genreService,
            ICountryService countryService,
            IPublicationService publicationService,
            IArtistSongWorkflowService artistSongWorkflowService,
            IArtistAlbumWorkflowService artistAlbumWorkflowService)
        {
            _artistService = artistService;
            _albumService = albumService;
            _songService = songService;
            _genreService = genreService;
            _countryService = countryService;
            _publicationService = publicationService;
            _artistSongWorkflowService = artistSongWorkflowService;
            _artistAlbumWorkflowService = artistAlbumWorkflowService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            var model = new ArtistDashboardViewModel
            {
                PopularSongs = dashboard.PopularSongs,
                RecentAlbums = dashboard.RecentAlbums,
                TotalStreams = dashboard.TotalStreams,
                TotalLikes = dashboard.TotalLikes,
                MonthlyListeners = dashboard.MonthlyListeners,
                TotalFollowers = dashboard.TotalFollowers
            };

            FillArtistLayoutData(model, dashboard);

            return View(model);
        }

        private bool TryGetDashboard(out ArtistDashboardDto dashboard, out int userId, out IActionResult? errorResult)
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

        private static void FillArtistLayoutData(ArtistLayoutViewModel model, ArtistDashboardDto dashboard)
        {
            model.Artist = dashboard.Artist;
            model.ArtistInitial = GetInitial(dashboard.Artist.Name);
            model.TotalAlbums = dashboard.TotalAlbums;
            model.TotalSongs = dashboard.TotalSongs;
        }

        private static string GetInitial(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "?";
            }

            return char.ToUpperInvariant(value.Trim()[0]).ToString();
        }
    }
}
