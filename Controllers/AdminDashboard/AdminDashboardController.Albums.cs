using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.Enums;
using MusicProject.ViewModels.AdminDashboard;

namespace MusicProject.Controllers
{
    public partial class AdminDashboardController
    {
        [HttpGet]
        public IActionResult Albums(string? search, PublicationStatus? status)
        {
            var albums = _adminDashboardService.GetAlbums(search, status).ToList();

            var model = new AdminAlbumsViewModel
            {
                SearchTerm = NormalizeSearch(search),
                StatusFilter = status,
                DisplayedAlbums = albums.Count,
                PublishedAlbums = albums.Count(album => album.PublicationStatus == PublicationStatus.Published),
                ScheduledAlbums = albums.Count(album => album.PublicationStatus == PublicationStatus.Scheduled),
                DraftAlbums = albums.Count(album => album.PublicationStatus == PublicationStatus.Draft),
                ArchivedAlbums = albums.Count(album => album.PublicationStatus == PublicationStatus.Archived),
                Albums = albums
            };

            FillLayoutData(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetAlbumAdminHiddenStatus(
            int albumId,
            bool isHidden,
            string? reason,
            string? search,
            PublicationStatus? status)
        {
            var result = _adminContentModerationService.SetAlbumAdminHiddenStatus(
                albumId,
                isHidden,
                reason);

            SetModerationMessage(result, isHidden, "Albüm");

            return RedirectToAction(nameof(Albums), new { search, status });
        }
    }
}
