using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.Enums;
using MusicProject.ViewModels.AdminDashboard;

namespace MusicProject.Controllers
{
    public partial class AdminDashboardController
    {
        [HttpGet]
        public IActionResult Songs(string? search, PublicationStatus? status)
        {
            var songs = _adminDashboardService.GetSongs(search, status).ToList();

            foreach (var song in songs)
            {
                song.ArtistName = OrFallback(song.ArtistName, "Sanatçı bilgisi yok");
                song.AlbumName = OrFallback(song.AlbumName, "Single");
                song.LabelName = OrFallback(song.LabelName, "Bağımsız");
            }

            var model = new AdminSongsViewModel
            {
                SearchTerm = NormalizeSearch(search),
                StatusFilter = status,
                DisplayedSongs = songs.Count,
                PublishedSongs = songs.Count(song => song.PublicationStatus == PublicationStatus.Published),
                ScheduledSongs = songs.Count(song => song.PublicationStatus == PublicationStatus.Scheduled),
                DraftSongs = songs.Count(song => song.PublicationStatus == PublicationStatus.Draft),
                ArchivedSongs = songs.Count(song => song.PublicationStatus == PublicationStatus.Archived),
                Songs = songs
            };

            FillLayoutData(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetSongAdminHiddenStatus(
            int songId,
            bool isHidden,
            string? reason,
            string? search,
            PublicationStatus? status)
        {
            var result = _adminContentModerationService.SetSongAdminHiddenStatus(
                songId,
                isHidden,
                reason);

            SetModerationMessage(result, isHidden, "Şarkı");

            return RedirectToAction(nameof(Songs), new { search, status });
        }
    }
}
