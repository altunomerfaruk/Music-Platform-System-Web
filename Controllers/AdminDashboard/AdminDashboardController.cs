using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Contracts.Responses.AdminDashboard;
using MusicProject.Models.Enums;
using MusicProject.Services.Interface;
using MusicProject.ViewModels.AdminDashboard;
using System.Globalization;
using System.Security.Claims;

namespace MusicProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminDashboardController : Controller
    {
        private static readonly CultureInfo TurkishCulture =
            CultureInfo.GetCultureInfo("tr-TR");

        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IAdminContentModerationService _adminContentModerationService;

        public AdminDashboardController(
            IAdminDashboardService adminDashboardService,
            IAdminContentModerationService adminContentModerationService)
        {
            _adminDashboardService = adminDashboardService;
            _adminContentModerationService = adminContentModerationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var dashboard = _adminDashboardService.GetDashboard();

            foreach (var user in dashboard.RecentUsers)
            {
                user.RoleName = user.Role.ToString();
                user.Initial = GetInitial(user.Username);
            }

            foreach (var song in dashboard.TopSongs)
            {
                song.ArtistName = OrFallback(song.ArtistName, "Sanatçı bilgisi yok");
            }

            FillWeeklyListeningChart(dashboard.WeeklyListenings);

            var model = new AdminDashboardViewModel
            {
                TotalListenings = dashboard.TotalListenings,
                RecentUsers = dashboard.RecentUsers,
                TopSongs = dashboard.TopSongs,
                WeeklyListenings = dashboard.WeeklyListenings
            };

            FillLayoutData(model);

            return View(model);
        }

        private void FillLayoutData(AdminLayoutViewModel model)
        {
            var totals = _adminDashboardService.GetLayoutTotals();

            model.TotalUsers = totals.TotalUsers;
            model.TotalArtists = totals.TotalArtists;
            model.TotalAlbums = totals.TotalAlbums;
            model.TotalSongs = totals.TotalSongs;
        }

        private static void FillWeeklyListeningChart(List<AdminDailyListeningDto> weeklyListenings)
        {
            if (weeklyListenings.Count == 0)
            {
                return;
            }

            var maximumListeningCount = weeklyListenings.Max(day => day.ListeningCount);

            foreach (var day in weeklyListenings)
            {
                day.DayLabel = day.Date.ToString("ddd", TurkishCulture);
                day.BarHeightPercent = CalculateBarHeight(day.ListeningCount, maximumListeningCount);
            }
        }

        private static int CalculateBarHeight(int listeningCount, int maximumListeningCount)
        {
            if (listeningCount == 0 || maximumListeningCount == 0)
            {
                return 0;
            }

            var percentage = (int)Math.Round(listeningCount * 100d / maximumListeningCount);

            return Math.Max(8, percentage);
        }

        private void SetModerationMessage(
            AdminContentVisibilityUpdateResult result,
            bool isHidden,
            string contentName)
        {
            if (result == AdminContentVisibilityUpdateResult.ContentNotFound)
            {
                TempData["ErrorMessage"] = $"{contentName} bulunamadı.";
                return;
            }

            if (result == AdminContentVisibilityUpdateResult.ReasonRequired)
            {
                TempData["ErrorMessage"] = $"{contentName} gizlenirken bir neden belirtmelisin.";
                return;
            }

            TempData["SuccessMessage"] = isHidden
                ? $"{contentName} admin tarafından gizlendi."
                : $"{contentName} üzerindeki admin gizlemesi kaldırıldı.";
        }

        private static string GetInitial(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "?";
            }

            return char.ToUpperInvariant(value.Trim()[0]).ToString();
        }

        private static string OrFallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string? NormalizeSearch(string? search)
        {
            return string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId);
        }
    }
}
