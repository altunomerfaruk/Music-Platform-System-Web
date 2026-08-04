using Microsoft.AspNetCore.Mvc;
using MusicProject.ViewModels.UserDashboard;

namespace MusicProject.Controllers
{
    // Album detay aksiyonlari.
    public partial class UserDashboardController
    {
        [HttpGet]
        public IActionResult AlbumDetails(int albumId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            if (albumId <= 0)
            {
                return BadRequest("Geçersiz albüm bilgisi.");
            }

            var album = _albumService.GetAlbumDetails(albumId);

            if (album == null)
            {
                return NotFound("Albüm bulunamadı.");
            }

            var model = new AlbumDetailsViewModel
            {
                Album = album,

                LikedSongIds = _likedSongService
                    .GetActiveLikedSongIds(userId)
                    .ToHashSet()
            };

            FillLayoutData(model, userId);

            return View(model);
        }
    }
}
